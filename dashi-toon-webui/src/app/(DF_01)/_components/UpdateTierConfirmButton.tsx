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
import { Edit, X } from "lucide-react";
import { editDashiFanTier } from "@/utils/api/dashifan";
export interface DeleteChapterButtonProps {
    isOpen?: boolean;
    seriesId: string;
    tierId: string;
    onConfirmed?: () => Promise<void>;
    onOpenChanged?: () => void;
}

export default function UpdateTierConfirmButton(
    props: DeleteChapterButtonProps,
) {
    const [isDeleting, setIsDeleting] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        props.onConfirmed && (await props.onConfirmed());
        setIsDeleting(false);
    };

    return (
        <Dialog open={props.isOpen} onOpenChange={props.onOpenChanged}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Xác nhận Thay Đổi Thông Tin Hạng</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc muốn thay đổi thông tin hạng hiện tại không?
                        Bạn chỉ có thể thay đổi lại sau 30 ngày!
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
                        className="flex gap-2"
                        variant="destructive"
                        onClick={handleDelete}
                        disabled={isDeleting}
                        type="submit"
                    >
                        {isDeleting ? (
                            <LoadingSpinner
                                className={cx({ hidden: !isDeleting })}
                            />
                        ) : (
                            <Edit className="mr-2 h-4 w-4" />
                        )}
                        Xác nhận
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
