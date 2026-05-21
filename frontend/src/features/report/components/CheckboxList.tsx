interface CheckboxListProps<T extends Record<string, boolean>> {
  options: Array<{ key: keyof T; label: string }>;
  values: T;
  onChange: (key: keyof T, checked: boolean) => void;
}

export function CheckboxList<T extends Record<string, boolean>>({
  options,
  values,
  onChange,
}: CheckboxListProps<T>) {
  return (
    <div className="flex flex-col gap-2">
      {options.map((option) => (
        <label
          key={String(option.key)}
          className="flex cursor-pointer items-center gap-2 font-heading text-[11px] font-medium text-text-primary sm:text-[12px]"
        >
          <input
            type="checkbox"
            checked={values[option.key]}
            onChange={(event) => onChange(option.key, event.target.checked)}
            className="h-4 w-4 accent-primary"
          />
          {option.label}
        </label>
      ))}
    </div>
  );
}
