import { Slider } from "@/app/components/ui/slider";
import { Label } from "@/app/components/ui/label";
import { Checkbox } from "@/app/components/ui/checkbox";
import { DISPOSITION_MIN, DISPOSITION_MAX, getDispositionLabel, getDispositionColor } from "@/utils/disposition";

interface DispositionSliderProps {
  value: number | null;
  onChange: (value: number | null) => void;
  id?: string;
}

export function DispositionSlider({ value, onChange, id = 'disposition-slider' }: DispositionSliderProps) {
  const enabled = value !== null;
  const displayValue = value ?? 0;
  const label = getDispositionLabel(value);
  const textColor = getDispositionColor(value);
  const scoreText = enabled ? `${displayValue > 0 ? '+' : ''}${displayValue}` : '';
  const labelId = `${id}-label`;

  return (
    <div className="grid gap-2">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Checkbox
            id={`${id}-toggle`}
            checked={enabled}
            onCheckedChange={(checked) => onChange(checked === true ? 0 : null)}
          />
          <Label id={labelId} htmlFor={`${id}-toggle`}>Disposition</Label>
        </div>
        <span className={`text-sm font-medium ${textColor}`}>
          {scoreText ? `${scoreText} ` : ''}{label}
        </span>
      </div>
      <Slider
        id={id}
        aria-labelledby={labelId}
        min={DISPOSITION_MIN}
        max={DISPOSITION_MAX}
        step={1}
        value={[displayValue]}
        onValueChange={([v]) => onChange(v)}
        disabled={!enabled}
        className="w-full"
      />
      <div className="flex justify-between text-xs text-muted-foreground">
        <span>Hostile</span>
        <span>Neutral</span>
        <span>Devoted</span>
      </div>
    </div>
  );
}
