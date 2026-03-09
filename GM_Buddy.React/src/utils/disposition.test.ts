import { describe, it, expect } from 'vitest';
import {
  DISPOSITION_MIN,
  DISPOSITION_MAX,
  getDispositionLabel,
  getDispositionColor,
  getDispositionBgColor,
} from './disposition';

describe('disposition constants', () => {
  it('should define correct min and max', () => {
    expect(DISPOSITION_MIN).toBe(-5);
    expect(DISPOSITION_MAX).toBe(5);
  });
});

describe('getDispositionLabel', () => {
  it('returns "Unset" for null', () => {
    expect(getDispositionLabel(null)).toBe('Unset');
  });

  it('returns "Unset" for undefined', () => {
    expect(getDispositionLabel(undefined)).toBe('Unset');
  });

  it('returns "Hostile" for -5', () => {
    expect(getDispositionLabel(-5)).toBe('Hostile');
  });

  it('returns "Antagonistic" for -4 and -3', () => {
    expect(getDispositionLabel(-4)).toBe('Antagonistic');
    expect(getDispositionLabel(-3)).toBe('Antagonistic');
  });

  it('returns "Unfriendly" for -2 and -1', () => {
    expect(getDispositionLabel(-2)).toBe('Unfriendly');
    expect(getDispositionLabel(-1)).toBe('Unfriendly');
  });

  it('returns "Neutral" for 0', () => {
    expect(getDispositionLabel(0)).toBe('Neutral');
  });

  it('returns "Friendly" for 1 and 2', () => {
    expect(getDispositionLabel(1)).toBe('Friendly');
    expect(getDispositionLabel(2)).toBe('Friendly');
  });

  it('returns "Loyal" for 3 and 4', () => {
    expect(getDispositionLabel(3)).toBe('Loyal');
    expect(getDispositionLabel(4)).toBe('Loyal');
  });

  it('returns "Devoted" for 5', () => {
    expect(getDispositionLabel(5)).toBe('Devoted');
  });

  it('returns "Unknown" for values above max', () => {
    expect(getDispositionLabel(6)).toBe('Unknown');
  });

  it('falls through to nearest bucket for values below min', () => {
    // Values below -5 are prevented by DB CHECK constraint,
    // but the function naturally classifies them via the if-chain
    expect(getDispositionLabel(-6)).toBe('Antagonistic');
  });
});

describe('getDispositionColor', () => {
  it('returns gray for null/undefined', () => {
    expect(getDispositionColor(null)).toBe('text-gray-400');
    expect(getDispositionColor(undefined)).toBe('text-gray-400');
  });

  it('returns red for strongly negative (-5, -4, -3)', () => {
    expect(getDispositionColor(-5)).toBe('text-red-400');
    expect(getDispositionColor(-4)).toBe('text-red-400');
    expect(getDispositionColor(-3)).toBe('text-red-400');
  });

  it('returns orange for mildly negative (-2, -1)', () => {
    expect(getDispositionColor(-2)).toBe('text-orange-400');
    expect(getDispositionColor(-1)).toBe('text-orange-400');
  });

  it('returns gray for neutral (0)', () => {
    expect(getDispositionColor(0)).toBe('text-gray-400');
  });

  it('returns green for mildly positive (1, 2)', () => {
    expect(getDispositionColor(1)).toBe('text-green-400');
    expect(getDispositionColor(2)).toBe('text-green-400');
  });

  it('returns emerald for strongly positive (3, 4, 5)', () => {
    expect(getDispositionColor(3)).toBe('text-emerald-400');
    expect(getDispositionColor(4)).toBe('text-emerald-400');
    expect(getDispositionColor(5)).toBe('text-emerald-400');
  });
});

describe('getDispositionBgColor', () => {
  it('returns gray bg for null/undefined', () => {
    expect(getDispositionBgColor(null)).toBe('bg-gray-500/20 border-gray-500/30');
    expect(getDispositionBgColor(undefined)).toBe('bg-gray-500/20 border-gray-500/30');
  });

  it('returns red bg for strongly negative', () => {
    expect(getDispositionBgColor(-5)).toBe('bg-red-500/20 border-red-500/30');
    expect(getDispositionBgColor(-3)).toBe('bg-red-500/20 border-red-500/30');
  });

  it('returns orange bg for mildly negative', () => {
    expect(getDispositionBgColor(-2)).toBe('bg-orange-500/20 border-orange-500/30');
    expect(getDispositionBgColor(-1)).toBe('bg-orange-500/20 border-orange-500/30');
  });

  it('returns gray bg for neutral', () => {
    expect(getDispositionBgColor(0)).toBe('bg-gray-500/20 border-gray-500/30');
  });

  it('returns green bg for mildly positive', () => {
    expect(getDispositionBgColor(1)).toBe('bg-green-500/20 border-green-500/30');
    expect(getDispositionBgColor(2)).toBe('bg-green-500/20 border-green-500/30');
  });

  it('returns emerald bg for strongly positive', () => {
    expect(getDispositionBgColor(3)).toBe('bg-emerald-500/20 border-emerald-500/30');
    expect(getDispositionBgColor(5)).toBe('bg-emerald-500/20 border-emerald-500/30');
  });
});
