import { Badge } from "@/app/components/ui/badge";
import { getDispositionLabel, getDispositionColor, getDispositionBgColor } from "@/types/npc";

interface DispositionBadgeProps {
  disposition: number | null | undefined;
  showScore?: boolean;
  className?: string;
}

export function DispositionBadge({ disposition, showScore = true, className = '' }: DispositionBadgeProps) {
  const label = getDispositionLabel(disposition);
  const textColor = getDispositionColor(disposition);
  const bgColor = getDispositionBgColor(disposition);

  const scoreText = disposition !== null && disposition !== undefined
    ? `${disposition > 0 ? '+' : ''}${disposition}`
    : '';

  return (
    <Badge
      variant="outline"
      className={`text-xs ${textColor} ${bgColor} ${className}`}
    >
      {showScore && scoreText ? `${scoreText} ` : ''}{label}
    </Badge>
  );
}
