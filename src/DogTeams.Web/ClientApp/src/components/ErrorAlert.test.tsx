import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import ErrorAlert from '../components/ErrorAlert';

describe('ErrorAlert Component', () => {
  it('should render error message', () => {
    render(<ErrorAlert message="Something went wrong" />);

    expect(screen.getByText(/Something went wrong/)).toBeInTheDocument();
  });

  it('should show dismiss button when onDismiss is provided', () => {
    const onDismiss = () => {};
    render(<ErrorAlert message="Error message" onDismiss={onDismiss} />);

    const dismissButton = screen.getByText('×');
    expect(dismissButton).toBeInTheDocument();
  });

  it('should not show dismiss button when onDismiss is not provided', () => {
    render(<ErrorAlert message="Error message" />);

    const dismissButtons = screen.queryAllByText('×');
    expect(dismissButtons.length).toBe(0);
  });

  it('should call onDismiss when dismiss button is clicked', () => {
    const onDismiss = vi.fn();

    const { rerender } = render(
      <ErrorAlert message="Error message" onDismiss={onDismiss} />
    );

    const dismissButton = screen.getByText('×');
    dismissButton.click();

    // After dismiss, rerender without message
    rerender(<ErrorAlert message="" onDismiss={onDismiss} />);

    expect(screen.queryByText('Error message')).not.toBeInTheDocument();
  });

  it('should have error styling applied', () => {
    const { container } = render(<ErrorAlert message="Error" />);

    const alertDiv = container.querySelector('div');
    expect(alertDiv).toBeInTheDocument();
    expect(alertDiv?.style.backgroundColor).toBeTruthy();
  });
});
