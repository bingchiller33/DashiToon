"use client";
import React, { useState } from "react";

import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { useRouter } from "next/navigation";

import PayPalUpgrade from "@/components/paypal/PayPalUpgrade";
import { toast } from "sonner";
export interface UpgradeDashiFanProps {
    seriesId: string;
    subscriptionId: string;
    tierId: string;
    children: React.ReactNode;
}

export default function UpgradeDashiFan(props: UpgradeDashiFanProps) {
    const router = useRouter();
    const [isDeleting, setIsDeleting] = useState(false);
    const [isOpen, setIsOpen] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        // const [_, err] = await removeChapter(
        //     props.seriesId,
        //     props.volumeId,
        //     props.chapterId,
        // );

        setIsDeleting(false);
        // if (err) {
        //     toast.error("Không xóa được chương");
        //     return;
        // }

        toast.success("Đã xóa chương thành công");
    };

    return (
        <Dialog>
            <DialogTrigger asChild>{props.children}</DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Xác nhận Nâng Cấp Hạng</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc chắn muốn nâng cấp hạng không? Bạn sẽ trả
                        phí chênh lệch giữa hạng cũ và hạng mới.
                    </DialogDescription>
                </DialogHeader>
                <DialogFooter className="sm:justify-unset gap-2 sm:flex-col">
                    <PayPalUpgrade
                        amount="1"
                        subId={props.subscriptionId}
                        tierId={props.tierId}
                        onSuccess={() => {
                            const url = new URL(window.location.href);
                            url.searchParams.delete("successPayment");
                            url.searchParams.delete("tierId");
                            url.searchParams.delete("ba_token");
                            url.searchParams.delete("subscription_id");
                            url.searchParams.delete("token");

                            url.searchParams.set("successPayment", "true");
                            url.searchParams.set("tierId", props.tierId);

                            window.location.href = url.toString();
                        }}
                    />
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
