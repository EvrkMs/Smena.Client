import { forwardRef, type ButtonHTMLAttributes, type InputHTMLAttributes, type SelectHTMLAttributes } from 'react';
import './primitives.css';

export function Button({
  variant = 'primary',
  className = '',
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: 'primary' | 'ghost' | 'danger' }) {
  return <button className={`btn btn-${variant} ${className}`} {...props} />;
}

export const TextField = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement> & { label?: string }>(
  ({ label, className = '', id, ...props }, ref) => {
    const inputId = id ?? label?.replace(/\s+/g, '-').toLowerCase();
    return (
      <label className="field" htmlFor={inputId}>
        {label && <span className="field-label">{label}</span>}
        <input ref={ref} id={inputId} className={`field-input ${className}`} {...props} />
      </label>
    );
  },
);
TextField.displayName = 'TextField';

/** Числовое поле: только цифры, ограничение по количеству знаков (как InputHelper.FilterNumericInput). */
export const NumberField = forwardRef<
  HTMLInputElement,
  Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'type'> & {
    label?: string;
    value: string;
    maxDigits?: number;
    onValueChange: (value: string) => void;
  }
>(({ label, value, maxDigits, onValueChange, className = '', id, ...props }, ref) => {
  const inputId = id ?? label?.replace(/\s+/g, '-').toLowerCase();
  return (
    <label className="field" htmlFor={inputId}>
      {label && <span className="field-label">{label}</span>}
      <input
        ref={ref}
        id={inputId}
        className={`field-input tabular ${className}`}
        inputMode="numeric"
        value={value}
        onChange={(e) => {
          let digits = e.target.value.replace(/\D/g, '');
          if (maxDigits) digits = digits.slice(0, maxDigits);
          onValueChange(digits);
        }}
        {...props}
      />
    </label>
  );
});
NumberField.displayName = 'NumberField';

export function Select({
  label,
  className = '',
  id,
  ...props
}: SelectHTMLAttributes<HTMLSelectElement> & { label?: string }) {
  const selectId = id ?? label?.replace(/\s+/g, '-').toLowerCase();
  return (
    <label className="field" htmlFor={selectId}>
      {label && <span className="field-label">{label}</span>}
      <select id={selectId} className={`field-input field-select ${className}`} {...props} />
    </label>
  );
}

export function Checkbox({
  label,
  checked,
  onChange,
}: {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
}) {
  return (
    <label className="checkbox">
      <input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} />
      <span className="checkbox-box" aria-hidden="true" />
      <span>{label}</span>
    </label>
  );
}

export function Panel({ title, children }: { title?: string; children: React.ReactNode }) {
  return (
    <section className="panel">
      {title && <h3 className="panel-title">{title}</h3>}
      {children}
    </section>
  );
}
