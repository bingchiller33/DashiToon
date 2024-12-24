import { useEffect, useState } from "react";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Input } from "@/components/ui/input";
import { Check } from "lucide-react";

export function ConfirmationModal({
    isOpen,
    onClose,
    onConfirm,
    itemName,
    entityType = "item",
    requiredText = "XOÁ",
    confirmTextPlaceholder = "Nhập vào để xác nhận",
    confirmDescription = "Hành động này là vĩnh viễn và không thể hoàn lại.",
}: ConfirmationModalProps) {
    const [confirmText, setConfirmText] = useState("");
    const [isValid, setIsValid] = useState(false);

    useEffect(() => {
        const [text1, text2] = requiredText.split(" / ");
        setIsValid(confirmText === text1 || confirmText === text2);
    }, [confirmText, requiredText]);

    return (
        <AlertDialog open={isOpen} onOpenChange={onClose}>
            <AlertDialogContent className="sm:max-w-[425px]">
                <AlertDialogHeader>
                    <AlertDialogTitle className="text-lg font-semibold">
                        Bạn sắp{" "}
                        <span className="text-red-500">xóa vĩnh viễn</span>{" "}
                        {entityType} này
                    </AlertDialogTitle>
                    <AlertDialogDescription className="text-gray-500">
                        Hành động này sẽ xóa &quot;{itemName}&quot;.{" "}
                        {confirmDescription}
                    </AlertDialogDescription>
                </AlertDialogHeader>

                <div className="mt-4">
                    <div className="mb-2 text-sm text-gray-500">
                        Nhập &apos;{requiredText.split(" / ")[0]}&apos; để xác
                        nhận
                    </div>
                    <div className="relative">
                        <Input
                            value={confirmText}
                            onChange={(e) => setConfirmText(e.target.value)}
                            className="pr-8"
                            placeholder={confirmTextPlaceholder}
                        />
                        {isValid && (
                            <Check className="absolute right-2 top-1/2 h-5 w-5 -translate-y-1/2 transform text-green-500" />
                        )}
                    </div>
                    <div className="mt-2 text-xs text-gray-400">
                        Tôi hiểu rằng {entityType} này sẽ bị xóa và không thể
                        khôi phục.
                    </div>
                </div>

                <AlertDialogFooter className="mt-6">
                    <AlertDialogCancel onClick={onClose}>Hủy</AlertDialogCancel>
                    <AlertDialogAction
                        onClick={onConfirm}
                        disabled={!isValid}
                        className={`${
                            isValid
                                ? "bg-red-500 hover:bg-red-600"
                                : "cursor-not-allowed bg-gray-300"
                        } text-white`}
                    >
                        Xóa
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}

export interface ConfirmationModalProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    itemName: string;
    entityType?: string;
    requiredText?: string;
    confirmTextPlaceholder?: string;
    confirmDescription?: string;
}
