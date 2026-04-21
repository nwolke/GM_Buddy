import { useCallback, useEffect, useRef, useState } from 'react';

export type AutoSaveStatus = 'idle' | 'saving' | 'saved' | 'error';

const SAVED_INDICATOR_DURATION_MS = 2000;

const getErrorMessage = (error: unknown): string => {
  if (error instanceof Error && error.message) {
    return error.message;
  }

  return 'Save failed';
};

export function useAutoSave<T>(saveFn: (value: T) => Promise<void>, delay = 300) {
  const saveFnRef = useRef(saveFn);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const savedResetTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const pendingValueRef = useRef<T | null>(null);
  const failedValueRef = useRef<T | null>(null);
  const statusRef = useRef<AutoSaveStatus>('idle');
  const mountedRef = useRef(true);

  const [status, setStatus] = useState<AutoSaveStatus>('idle');
  const [error, setError] = useState<string | null>(null);

  const setStatusSafely = useCallback((nextStatus: AutoSaveStatus) => {
    statusRef.current = nextStatus;
    if (mountedRef.current) {
      setStatus(nextStatus);
    }
  }, []);

  useEffect(() => {
    saveFnRef.current = saveFn;
  }, [saveFn]);

  const runSave = useCallback(async (value: T) => {
    setStatusSafely('saving');
    if (mountedRef.current) {
      setError(null);
    }

    try {
      await saveFnRef.current(value);
      failedValueRef.current = null;
      setStatusSafely('saved');

      if (savedResetTimerRef.current) {
        clearTimeout(savedResetTimerRef.current);
      }

      savedResetTimerRef.current = setTimeout(() => {
        setStatusSafely('idle');
      }, SAVED_INDICATOR_DURATION_MS);
    } catch (err) {
      failedValueRef.current = value;
      if (mountedRef.current) {
        setError(getErrorMessage(err));
      }
      setStatusSafely('error');
      throw err;
    }
  }, [setStatusSafely]);

  const flush = useCallback(async () => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }

    const pendingValue = pendingValueRef.current;
    if (pendingValue === null) {
      return;
    }

    pendingValueRef.current = null;
    await runSave(pendingValue);
  }, [runSave]);

  const scheduleSave = useCallback((value: T) => {
    pendingValueRef.current = value;

    if (savedResetTimerRef.current) {
      clearTimeout(savedResetTimerRef.current);
      savedResetTimerRef.current = null;
    }

    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }

    setStatusSafely('saving');
    if (mountedRef.current) {
      setError(null);
    }

    timerRef.current = setTimeout(() => {
      timerRef.current = null;
      const valueToSave = pendingValueRef.current;
      pendingValueRef.current = null;

      if (valueToSave !== null) {
        void runSave(valueToSave).catch(() => undefined);
      }
    }, delay);
  }, [delay, runSave, setStatusSafely]);

  const retry = useCallback(() => {
    if (failedValueRef.current !== null) {
      scheduleSave(failedValueRef.current);
    }
  }, [scheduleSave]);

  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!timerRef.current && statusRef.current !== 'saving') {
        return;
      }

      event.preventDefault();
      event.returnValue = '';
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [flush]);

  useEffect(() => {
    return () => {
      mountedRef.current = false;

      if (timerRef.current) {
        clearTimeout(timerRef.current);
        timerRef.current = null;
      }

      if (savedResetTimerRef.current) {
        clearTimeout(savedResetTimerRef.current);
        savedResetTimerRef.current = null;
      }

      const pendingValue = pendingValueRef.current;
      if (pendingValue !== null) {
        pendingValueRef.current = null;
        void saveFnRef.current(pendingValue).catch(() => undefined);
      }
    };
  }, []);

  return {
    status,
    error,
    scheduleSave,
    flush,
    retry,
    hasPendingSave: timerRef.current !== null || status === 'saving',
  };
}
