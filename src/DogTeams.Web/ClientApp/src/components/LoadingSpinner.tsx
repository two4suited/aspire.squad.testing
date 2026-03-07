export default function LoadingSpinner() {
  return (
    <div style={{
      display: 'inline-block',
      animation: 'spin 1s linear infinite',
      marginRight: 8,
    }}>
      ⟳
    </div>
  );
}
