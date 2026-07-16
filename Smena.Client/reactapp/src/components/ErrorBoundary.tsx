import { Component, type ErrorInfo, type ReactNode } from 'react';

interface State {
  error: Error | null;
}

export class ErrorBoundary extends Component<{ children: ReactNode }, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    // eslint-disable-next-line no-console
    console.error('React render error:', error, info.componentStack);
  }

  render() {
    if (this.state.error) {
      return (
        <div
          style={{
            height: '100vh',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 12,
            background: '#161d27',
            color: '#e7ebf0',
            fontFamily: 'Segoe UI, sans-serif',
            padding: 24,
            textAlign: 'center',
          }}
        >
          <div style={{ fontSize: 20, color: '#c0604a' }}>Что-то пошло не так</div>
          <div style={{ fontSize: 13, color: '#8b97a7', maxWidth: 520 }}>{this.state.error.message}</div>
          <button
            onClick={() => this.setState({ error: null })}
            style={{
              marginTop: 8,
              padding: '8px 18px',
              background: '#c08b3d',
              border: 'none',
              borderRadius: 6,
              color: '#191307',
              fontWeight: 600,
              cursor: 'pointer',
            }}
          >
            Попробовать снова
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
