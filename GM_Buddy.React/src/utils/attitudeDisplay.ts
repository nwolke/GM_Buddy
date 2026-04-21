export interface AttitudeDisplay {
  label: string;
  colorClass: string;
}

export function getAttitudeDisplay(score: number): AttitudeDisplay {
  if (score <= -4) {
    return { label: 'Hostile', colorClass: 'text-red-400' };
  }

  if (score <= -2) {
    return { label: 'Unfriendly', colorClass: 'text-orange-400' };
  }

  if (score <= 1) {
    return { label: 'Neutral', colorClass: 'text-muted-foreground' };
  }

  if (score <= 3) {
    return { label: 'Friendly', colorClass: 'text-green-400' };
  }

  return { label: 'Devoted', colorClass: 'text-emerald-300' };
}
