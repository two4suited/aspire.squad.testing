import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import Modal from '../components/Modal';

describe('Modal Component', () => {
  it('should not render when isOpen is false', () => {
    render(
      <Modal isOpen={false} title="Test Modal" onClose={() => {}}>
        <p>Modal content</p>
      </Modal>
    );

    expect(screen.queryByText('Test Modal')).not.toBeInTheDocument();
  });

  it('should render when isOpen is true', () => {
    render(
      <Modal isOpen={true} title="Test Modal" onClose={() => {}}>
        <p>Modal content</p>
      </Modal>
    );

    expect(screen.getByText('Test Modal')).toBeInTheDocument();
    expect(screen.getByText('Modal content')).toBeInTheDocument();
  });

  it('should call onClose when close button is clicked', () => {
    const onClose = vi.fn();

    const { rerender } = render(
      <Modal isOpen={true} title="Test Modal" onClose={onClose}>
        <p>Modal content</p>
      </Modal>
    );

    const closeButton = screen.getByText('×');
    closeButton.click();

    // After clicking close, rerender with isOpen false
    rerender(
      <Modal isOpen={false} title="Test Modal" onClose={onClose}>
        <p>Modal content</p>
      </Modal>
    );

    expect(screen.queryByText('Test Modal')).not.toBeInTheDocument();
  });

  it('should render children correctly', () => {
    render(
      <Modal isOpen={true} title="Test Modal" onClose={() => {}}>
        <div>
          <h3>Child content</h3>
          <button>Action button</button>
        </div>
      </Modal>
    );

    expect(screen.getByText('Child content')).toBeInTheDocument();
    expect(screen.getByText('Action button')).toBeInTheDocument();
  });
});
