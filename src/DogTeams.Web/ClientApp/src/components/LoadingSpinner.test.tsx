import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import LoadingSpinner from '../components/LoadingSpinner';

describe('LoadingSpinner Component', () => {
  it('should render loading spinner', () => {
    render(<LoadingSpinner />);

    const spinner = screen.getByText('⟳');
    expect(spinner).toBeInTheDocument();
  });

  it('should have animation styles', () => {
    const { container } = render(<LoadingSpinner />);

    const div = container.querySelector('div');
    expect(div).toHaveStyle({
      display: 'inline-block',
      marginRight: '8px',
    });
  });
});
