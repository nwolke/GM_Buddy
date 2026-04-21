import { AutoSaveStatus } from '@/hooks/useAutoSave';

interface SaveIndicatorProps {
  status: AutoSaveStatus;
  error?: string | null;
  onRetry?: () => void;
}

export function SaveIndicator({ status, error, onRetry }: SaveIndicatorProps) {
  if (status === 'idle') {
    return null;
  }

  if (status === 'saving') {
    return <span className="text-[12px] text-muted-foreground">Saving...</span>;
  }

  if (status === 'saved') {
    return <span className="text-[12px] text-emerald-400">Saved ✓</span>;
  }

  return (
    <span className="text-[12px] text-red-400">
      Save failed
      {onRetry && (
        <button
          type="button"
          className="ml-1 underline underline-offset-2"
          onClick={onRetry}
        >
          Retry
        </button>
      )}
      {error ? <span className="sr-only">: {error}</span> : null}
    </span>
  );
}
