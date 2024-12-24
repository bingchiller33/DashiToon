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

import cx from "classnames";
import { toast } from "sonner";
import { cancelSubscription } from "@/utils/api/subscription";
import { X } from "lucide-react";
export interface DeleteChapterButtonProps {
    subId: string;
    onSubCanceled?: () => void;
}

export default function CancelSubscriptionButton(
    props: DeleteChapterButtonProps,
) {
    const [isDeleting, setIsDeleting] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        const [_, err] = await cancelSubscription(props.subId);

        setIsDeleting(false);
        if (err) {
            toast.error("Không hủy gói được, vui lòng thử lại sau ít phút");
            return;
        }

        toast.success("Đã hủy gói thành công");
        props.onSubCanceled && props.onSubCanceled();
        window.location.reload();
    };

    return (
        <Dialog>
            <DialogTrigger asChild>
                <Button
                    variant="destructive"
                    className="w-full bg-red-700 hover:bg-red-600"
                >
                    <X className="mr-2 h-4 w-4" /> Hủy đăng ký
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Xác nhận Hủy Gói DashiFan</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc muốn hủy gói DashiFan hiện tại không? Bạn sẽ
                        vẫn được hưởng những lợi ích của gói này đến hết kỳ hạn
                        đăng ký.
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
                            <X className="mr-2 h-4 w-4" />
                        )}
                        Xác nhận
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
