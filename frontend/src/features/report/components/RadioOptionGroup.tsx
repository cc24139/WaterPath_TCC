interface RadioOptionGroupProps<T extends string> {
  name: string;
  options: Array<{ value: T; label: string }>;
  value: T;
  onChange: (value: T) => void;
}

export function RadioOptionGroup<T extends string>({
  name,
  options,
  value,
  onChange,
}: RadioOptionGroupProps<T>) {
  return (
    <div className="flex flex-wrap justify-center items-center gap-x-5 gap-y-3">
      {options.map((option) => (
        <label
          key={option.value}
          className="flex cursor-pointer items-center gap-2 font-heading text-[11px] font-medium text-text-primary sm:text-[12px]"
        >
          <input
            type="radio"
            name={name}
            value={option.value}
            checked={value === option.value}
            onChange={() => onChange(option.value)}
            className="peer sr-only"
          />
          <span className="flex h-4 w-4 items-center justify-center rounded-full border border-placeholder bg-white peer-checked:border-primary">
            <span
              className={`h-2.5 w-2.5 rounded-full bg-primary ${
                value === option.value ? "block" : "hidden"
              }`}
            />
          </span>
          {option.label}
        </label>
      ))}
    </div>
  );
}
