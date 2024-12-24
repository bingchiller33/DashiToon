import { useLocalStorage } from "usehooks-ts";

export default function useUwU(gate: string = "uwu") {
    return useLocalStorage(gate, false, { initializeWithValue: false });
}
