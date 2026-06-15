import type { ChangeEvent, InputHTMLAttributes } from "react";
import { LuChevronDown } from "react-icons/lu";

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
  type = "text",
  inputMode,
  options,
  rows = 3,
  maxLength,
  className = "",
  isTextarea = false,
}: AddAnalysisFieldProps) {
  const fieldId = `add-analysis-${name}`;
  const fieldClassName =
    "w-full rounded-md border border-placeholder bg-white px-3.5 font-heading text-[12px] font-medium text-text-primary outline-none transition-colors placeholder:text-[#8A8A8A] focus:border-primary focus:ring-2 focus:ring-secondary/25";

  function handleChange(
    event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>
  ) {
    onChange(name, event.target.value);
  }

  return (
    <label className={`block min-w-0 ${className}`} htmlFor={fieldId}>
      <span className="mb-2 block font-heading text-[12px] font-bold text-text-primary">
        {label}
      </span>

      {options ? (
        <span className="relative block">
          <select
            id={fieldId}
            name={name}
            value={value}
            onChange={handleChange}
            className={`${fieldClassName} h-11 appearance-none pr-10`}
          >
            <option value="">{placeholder}</option>
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
            id={fieldId}
            name={name}
            value={value}
            onChange={handleChange}
            rows={rows}
            maxLength={maxLength}
            placeholder={placeholder}
            className={`${fieldClassName} min-h-20 resize-none py-3 pr-14 leading-relaxed`}
          />
          {maxLength ? (
            <span className="absolute bottom-2 right-3 font-heading text-[10px] font-medium text-text-secondary">
              {value.length}/{maxLength}
            </span>
          ) : null}
        </span>
      ) : (
        <input
          id={fieldId}
          name={name}
          value={value}
          onChange={handleChange}
          type={type}
          inputMode={inputMode}
          placeholder={placeholder}
          className={`${fieldClassName} h-11`}
        />
      )}

      {helper ? (
        <span className="mt-1.5 block font-heading text-[10px] font-medium text-text-secondary">
          {helper}
        </span>
      ) : null}
    </label>
  );
}
