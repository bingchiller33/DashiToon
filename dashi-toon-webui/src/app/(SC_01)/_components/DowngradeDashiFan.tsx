"use client";
import { Button } from "@/components/ui/button";
import { LoadingSpinner } from "@/components/ui/spinner";
import React, { useState } from "react";

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

import { downgrade } from "@/utils/api/subscription";
import cx from "classnames";
import { ChevronsDown } from "lucide-react";
import { toast } from "sonner";
export interface UpgradeDashiFanProps {
    seriesId: string;
    subscriptionId: string;
    tierId: string;
    children: React.ReactNode;
}

export default function DowngradeDashiFan(props: UpgradeDashiFanProps) {
    const [isDeleting, setIsDeleting] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        const [_, err] = await downgrade(props.tierId, props.subscriptionId);

        setIsDeleting(false);
        if (err) {
            toast.error("Không hạ cấp hạng được, vui lòng thử lại sau");
            return;
        }

        toast.success("Đã xóa chương thành công");
        window.location.reload();
    };

    return (
        <Dialog>
            <DialogTrigger asChild>{props.children}</DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Xác nhận Hạ Cấp Hạng</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc chắn muốn hạ cấp hạng không? Bạn vẫn sẽ
                        hường quyền lợi của hạng cũ cho đến hết kỳ thanh toán.
                    </DialogDescription>
                </DialogHeader>
                <DialogFooter className="">
                    <DialogClose className="mr-2" asChild>
                        <Button variant="outline">Hủy</Button>
                    </DialogClose>
                    <Button
                        className={cx(
                            "bg-red-500 text-white",
                            "hover:bg-red-600",
                        )}
                        onClick={handleDelete}
                        disabled={isDeleting}
                    >
                        {isDeleting ? (
                            <LoadingSpinner />
                        ) : (
                            <>
                                <ChevronsDown className="mr-2" />
                                Xác nhận
                            </>
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
