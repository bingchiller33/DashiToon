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
import { editDashiFanTier, editDashiFanTierStatus } from "@/utils/api/dashifan";
export interface DeleteChapterButtonProps {
    seriesId: string;
    tierId: string;
    onDeleted?: () => void;
}

export default function DeleteTierButton(props: DeleteChapterButtonProps) {
    const [isDeleting, setIsDeleting] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        const [_, err] = await editDashiFanTierStatus(
            props.seriesId,
            props.tierId,
        );

        setIsDeleting(false);
        if (err) {
            toast.error("Không xóa được hạng DashiFan, vui lòng thử lại sau");
            return;
        }

        toast.success("Đã xóa hạng DashiFan thành công");
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
                    <DialogTitle>Xác nhận xóa hạng DashiFan</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc chắn muốn xóa hạng DashiFan này không? Sau
                        khi xóa, các DashiFan của bạn sẽ vẫn duy trì đăng ký cho
                        đến khi chu kỳ hiện tại kết thúc. Hành động này không
                        thể hoàn tác.
                    </DialogDescription>
                </DialogHeader>
                <DialogFooter className="gap-2 sm:justify-end">
                    <DialogClose asChild>
                        <Button
                            type="button"
                            variant="ghost"
                            disabled={isDeleting}
                        >
                            Hủy
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
                        Xóa hạng DashiFan!
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
