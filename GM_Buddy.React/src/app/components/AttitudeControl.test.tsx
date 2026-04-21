import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { AttitudeControl } from '@/app/components/AttitudeControl';

describe('AttitudeControl', () => {
  it('disables decrement at minimum boundary', () => {
    render(
      <AttitudeControl
        score={-5}
        onIncrement={vi.fn()}
        onDecrement={vi.fn()}
      />
    );

    expect(screen.getByRole('button', { name: 'Decrease attitude score' })).toBeDisabled();
  });

  it('calls increment and decrement handlers', () => {
    const onIncrement = vi.fn();
    const onDecrement = vi.fn();

    render(
      <AttitudeControl
        score={0}
        onIncrement={onIncrement}
        onDecrement={onDecrement}
      />
    );

    fireEvent.click(screen.getByRole('button', { name: 'Increase attitude score' }));
    fireEvent.click(screen.getByRole('button', { name: 'Decrease attitude score' }));

    expect(onIncrement).toHaveBeenCalledTimes(1);
    expect(onDecrement).toHaveBeenCalledTimes(1);
  });
});
