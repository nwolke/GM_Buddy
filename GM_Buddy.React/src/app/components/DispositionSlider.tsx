import { Slider } from "@/app/components/ui/slider";
import { Label } from "@/app/components/ui/label";
import { DISPOSITION_MIN, DISPOSITION_MAX, getDispositionLabel, getDispositionColor } from "@/types/npc";

interface DispositionSliderProps {
  value: number;
  onChange: (value: number) => void;
  id?: string;
}

export function DispositionSlider({ value, onChange, id = 'disposition-slider' }: DispositionSliderProps) {
  const label = getDispositionLabel(value);
  const textColor = getDispositionColor(value);
  const scoreText = `${value > 0 ? '+' : ''}${value}`;

  return (
    <div className="grid gap-2">
      <div className="flex items-center justify-between">
        <Label htmlFor={id}>Disposition</Label>
        <span className={`text-sm font-medium ${textColor}`}>
          {scoreText} {label}
        </span>
      </div>
      <Slider
        id={id}
        min={DISPOSITION_MIN}
        max={DISPOSITION_MAX}
        step={1}
        value={[value]}
        onValueChange={([v]) => onChange(v)}
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
