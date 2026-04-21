import { act, renderHook } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import { useAutoSave } from '@/hooks/useAutoSave';

describe('useAutoSave', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('debounces and saves the latest value', async () => {
    const saveFn = vi.fn(async () => Promise.resolve());
    const { result } = renderHook(() => useAutoSave(saveFn, 300));

    act(() => {
      result.current.scheduleSave('first');
      result.current.scheduleSave('second');
    });

    await act(async () => {
      await vi.advanceTimersByTimeAsync(300);
    });

    expect(saveFn).toHaveBeenCalledTimes(1);
    expect(saveFn).toHaveBeenCalledWith('second');
    expect(result.current.status).toBe('saved');
  });

  it('marks beforeunload when there is a pending save', async () => {
    const saveFn = vi.fn(async () => Promise.resolve());
    const { result } = renderHook(() => useAutoSave(saveFn, 300));

    act(() => {
      result.current.scheduleSave('pending');
    });

    const event = new Event('beforeunload', { cancelable: true });
    Object.defineProperty(event, 'returnValue', {
      writable: true,
      configurable: true,
      value: undefined,
    });
    window.dispatchEvent(event);

    expect((event as Event & { returnValue?: string }).returnValue).toBe('');
    expect(saveFn).not.toHaveBeenCalled();
  });

  it('supports retry after save failure', async () => {
    const saveFn = vi.fn(async (_value: string) => Promise.resolve());
    saveFn.mockRejectedValueOnce(new Error('network down'));
    saveFn.mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useAutoSave(saveFn, 300));

    act(() => {
      result.current.scheduleSave('value');
    });

    await act(async () => {
      await vi.advanceTimersByTimeAsync(300);
    });

    expect(result.current.status).toBe('error');

    act(() => {
      result.current.retry();
    });

    await act(async () => {
      await vi.advanceTimersByTimeAsync(300);
    });

    expect(saveFn).toHaveBeenCalledTimes(2);
    expect(result.current.status).toBe('saved');
  });
});
