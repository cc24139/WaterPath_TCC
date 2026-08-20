import type { InputHTMLAttributes } from "react";
import { LuChevronDown } from "react-icons/lu";

export interface FormFieldOption {
  label: string;
  value: string;
}

interface FormFieldProps {
  id: string;
  label: string;
  name: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  helper?: string;
  error?: string;
  type?: InputHTMLAttributes<HTMLInputElement>["type"];
  inputMode?: InputHTMLAttributes<HTMLInputElement>["inputMode"];
  autoComplete?: InputHTMLAttributes<HTMLInputElement>["autoComplete"];
  min?: InputHTMLAttributes<HTMLInputElement>["min"];
  step?: InputHTMLAttributes<HTMLInputElement>["step"];
  options?: FormFieldOption[];
  rows?: number;
  maxLength?: number;
  className?: string;
  isTextarea?: boolean;
  required?: boolean;
}

export function FormField({
  id,
  label,
  name,
  value,
  onChange,
  placeholder = "",
  helper,
  error,
  type = "text",
  inputMode,
  autoComplete,
  min,
  step,
  options,
  rows = 3,
  maxLength,
  className = "",
  isTextarea = false,
  required = false,
}: FormFieldProps) {
  const helperId = helper ? `${id}-helper` : undefined;
  const errorId = error ? `${id}-error` : undefined;
  const describedBy = [helperId, errorId].filter(Boolean).join(" ") || undefined;
  const fieldClassName = `w-full rounded-md border bg-white px-3.5 font-heading text-[12px] font-medium text-text-primary outline-none transition-colors placeholder:text-text-secondary/70 focus:ring-2 ${
    error
      ? "border-contrast focus:border-contrast focus:ring-contrast/15"
      : "border-placeholder focus:border-primary focus:ring-secondary/25"
  }`;

  return (
    <div className={`min-w-0 ${className}`}>
      <label
        htmlFor={id}
        className="mb-2 block font-heading text-[12px] font-bold text-text-primary"
      >
        {label}
      </label>

      {options ? (
        <span className="relative block">
          <select
            id={id}
            name={name}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            required={required}
            aria-invalid={Boolean(error)}
            aria-describedby={describedBy}
            className={`${fieldClassName} h-11 appearance-none pr-10`}
          >
            {placeholder ? <option value="">{placeholder}</option> : null}
            {options.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          <LuChevronDown className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-text-secondary" />
        </span>
      ) : isTextarea ? (
        <span className="relative block">
          <textarea
            id={id}
            name={name}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            rows={rows}
            maxLength={maxLength}
            placeholder={placeholder}
            required={required}
            aria-invalid={Boolean(error)}
            aria-describedby={describedBy}
            className={`${fieldClassName} min-h-24 resize-y py-3 ${
              maxLength ? "pb-7" : ""
            } leading-relaxed`}
          />
          {maxLength ? (
            <span className="pointer-events-none absolute bottom-2 right-3 font-heading text-[10px] font-medium text-text-secondary">
              {value.length}/{maxLength}
            </span>
          ) : null}
        </span>
      ) : (
        <input
          id={id}
          name={name}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          type={type}
          inputMode={inputMode}
          autoComplete={autoComplete}
          min={min}
          step={step}
          placeholder={placeholder}
          required={required}
          aria-invalid={Boolean(error)}
          aria-describedby={describedBy}
          className={`${fieldClassName} h-11`}
        />
      )}

      {helper ? (
        <p
          id={helperId}
          className="mt-1.5 font-heading text-[10px] font-medium text-text-secondary"
        >
          {helper}
        </p>
      ) : null}

      {error ? (
        <p
          id={errorId}
          role="alert"
          className="mt-1.5 font-heading text-[11px] font-semibold text-contrast"
        >
          {error}
        </p>
      ) : null}
    </div>
  );
}
