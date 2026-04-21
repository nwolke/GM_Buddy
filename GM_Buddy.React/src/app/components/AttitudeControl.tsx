import { getAttitudeDisplay } from '@/utils/attitudeDisplay';

interface AttitudeControlProps {
  score: number;
  onIncrement: () => void;
  onDecrement: () => void;
  showErrorFlash?: boolean;
}

export function AttitudeControl({
  score,
  onIncrement,
  onDecrement,
  showErrorFlash = false,
}: AttitudeControlProps) {
  const atMin = score <= -5;
  const atMax = score >= 5;
  const display = getAttitudeDisplay(score);

  return (
    <div className="inline-flex items-center gap-1">
      <button
        type="button"
        className="h-11 w-11 rounded-full border border-border/70 hover:bg-gray-200 hover:text-foreground active:scale-95 disabled:cursor-not-allowed disabled:opacity-50"
        onClick={onDecrement}
        disabled={atMin}
        aria-label="Decrease attitude score"
      >
        -
      </button>
      <div className="min-w-[72px] text-center">
        <p className={`font-semibold tabular-nums ${display.colorClass} ${showErrorFlash ? 'animate-pulse text-red-400' : ''}`}>
          {score > 0 ? '+' : ''}{score}
        </p>
        <p className="text-[11px] text-muted-foreground">{display.label}</p>
      </div>
      <button
        type="button"
        className="h-11 w-11 rounded-full border border-border/70 hover:bg-gray-200 hover:text-foreground active:scale-95 disabled:cursor-not-allowed disabled:opacity-50"
        onClick={onIncrement}
        disabled={atMax}
        aria-label="Increase attitude score"
      >
        +
      </button>
    </div>
  );
}
