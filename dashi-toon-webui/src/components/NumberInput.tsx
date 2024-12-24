import { Button } from "./ui/button";
import { Input } from "./ui/input";

export interface NumberInputProps {
    value: number;
    onChange: (value: number) => void;
    placeholder?: string;
}

export function NumberInput(props: NumberInputProps) {
    const { value, onChange, placeholder } = props;

    return (
        <div className="flex">
            <Input
                type="number"
                placeholder={placeholder}
                className="max-w-24 rounded-r-none"
                value={value}
                onChange={(e) => onChange(parseInt(e.target.value))}
            />
            <Button
                className="rounded-none border-l-transparent"
                variant={"outline"}
                onClick={() => onChange(value - 1)}
            >
                -
            </Button>
            <Button
                className="rounded-l-none border-l-transparent"
                variant={"outline"}
                onClick={() => onChange(value + 1)}
            >
                +
            </Button>
        </div>
    );
}
