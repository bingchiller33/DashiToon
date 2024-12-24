"use client";
import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { LoadingSpinner } from "@/components/ui/spinner";

import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { FaTrash } from "react-icons/fa";
import { useRouter } from "next/navigation";

import cx from "classnames";
import { removeChapter } from "@/utils/api/author-studio/chapter";
import { toast } from "sonner";
export interface DeleteChapterButtonProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;
    onDeleted?: () => void;
}

export default function DeleteChapterButton(props: DeleteChapterButtonProps) {
    const router = useRouter();
    const [isDeleting, setIsDeleting] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        const [_, err] = await removeChapter(
            props.seriesId,
            props.volumeId,
            props.chapterId,
        );

        setIsDeleting(false);
        if (err) {
            toast.error("Không xóa được chương");
            return;
        }

        toast.success("Đã xóa chương thành công");
        props.onDeleted && props.onDeleted();
    };

    return (
        <Dialog>
            <DialogTrigger asChild>
                <Button className="flex gap-2 bg-red-700 text-white">
                    <FaTrash />
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Xác nhận Xóa Chương</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc chắn muốn xóa chương không? Hành động này
                        không thể hoàn tác.
                    </DialogDescription>
                </DialogHeader>
                <DialogFooter className="gap-2 sm:justify-end">
                    <DialogClose asChild>
                        <Button
                            type="button"
                            variant="ghost"
                            disabled={isDeleting}
                        >
                            Hủy bỏ
                        </Button>
                    </DialogClose>
                    <Button
                        variant={"destructive"}
                        className="flex gap-2"
                        onClick={handleDelete}
                        disabled={isDeleting}
                    >
                        {isDeleting ? (
                            <LoadingSpinner
                                className={cx({ hidden: !isDeleting })}
                            />
                        ) : (
                            <FaTrash />
                        )}
                        Xóa chương!
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
