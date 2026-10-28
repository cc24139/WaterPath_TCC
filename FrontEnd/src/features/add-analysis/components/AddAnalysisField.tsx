import type { InputHTMLAttributes } from "react";

import { FormField } from "@/components/ui/FormField";
import { DateTimeField } from "@/components/ui/date/DateTimeField";
import type {
  AddAnalysisFieldName,
  SelectOption,
} from "@/features/add-analysis/types/addAnalysis";

interface AddAnalysisFieldProps {
  label: string;
  name: AddAnalysisFieldName;
  value: string;
  onChange: (name: AddAnalysisFieldName, value: string) => void;
  placeholder?: string;
  helper?: string;
  error?: string;
  disabled?: boolean;
  required?: boolean;
  type?: InputHTMLAttributes<HTMLInputElement>["type"];
  inputMode?: InputHTMLAttributes<HTMLInputElement>["inputMode"];
  options?: SelectOption[];
  rows?: number;
  maxLength?: number;
  className?: string;
  isTextarea?: boolean;
}

export function AddAnalysisField({
  label,
  name,
  value,
  onChange,
  placeholder = "",
  helper,
  error,
  disabled,
  required,
  type = "text",
  inputMode,
  options,
  rows = 3,
  maxLength,
  className = "",
  isTextarea = false,
}: AddAnalysisFieldProps) {
  const fieldId = `add-analysis-${name}`;

  if (type === "datetime-local") return <DateTimeField id={fieldId} label={label} value={value}
    onChange={next => onChange(name, next)} helper={helper} error={error} disabled={disabled} required={required} className={className} />;

  return (
    <FormField
      id={fieldId}
      label={label}
      name={name}
      value={value}
      onChange={(nextValue) => onChange(name, nextValue)}
      placeholder={placeholder}
      helper={helper}
      error={error}
      disabled={disabled}
      required={required}
      type={type}
      inputMode={inputMode}
      options={options}
      rows={rows}
      maxLength={maxLength}
      className={className}
      isTextarea={isTextarea}
    />
  );
}
