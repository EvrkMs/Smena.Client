import { createContext, useCallback, useContext, useState, type ReactNode } from 'react';
import { Button } from './primitives';
import './confirm.css';

interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

const ConfirmContext = createContext<((opts: ConfirmOptions) => Promise<boolean>) | null>(null);

export function useConfirm() {
  const ctx = useContext(ConfirmContext);
  if (!ctx) throw new Error('useConfirm must be used within ConfirmProvider');
  return ctx;
}

export function ConfirmProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<{ opts: ConfirmOptions; resolve: (v: boolean) => void } | null>(null);

  const confirm = useCallback((opts: ConfirmOptions) => {
    return new Promise<boolean>((resolve) => setState({ opts, resolve }));
  }, []);

  const close = (result: boolean) => {
    state?.resolve(result);
    setState(null);
  };

  return (
    <ConfirmContext.Provider value={confirm}>
      {children}
      {state && (
        <div className="confirm-backdrop" onClick={() => close(false)}>
          <div className="confirm-slip" onClick={(e) => e.stopPropagation()}>
            <div className="confirm-slip-perforation" aria-hidden="true" />
            <h4>{state.opts.title}</h4>
            <p>{state.opts.message}</p>
            <div className="confirm-actions">
              <Button variant="ghost" onClick={() => close(false)}>
                {state.opts.cancelLabel ?? 'Отмена'}
              </Button>
              <Button variant={state.opts.danger ? 'danger' : 'primary'} onClick={() => close(true)} autoFocus>
                {state.opts.confirmLabel ?? 'Подтвердить'}
              </Button>
            </div>
          </div>
        </div>
      )}
    </ConfirmContext.Provider>
  );
}
