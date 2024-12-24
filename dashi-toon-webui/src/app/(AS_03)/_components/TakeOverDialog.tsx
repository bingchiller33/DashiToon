import React from "react";
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
import { Button } from "@/components/ui/button";

export default function TakeOverDialog() {
    return (
        <div className="flex flex-wrap items-center justify-center gap-2 bg-neutral-700 py-2">
            <p className="w-full basis-96 text-center">
                Ai đó đang chỉnh sửa, bạn có muốn tiếp quản không?
            </p>
            <Dialog>
                <DialogTrigger asChild>
                    <Button className="h-auto w-full basis-40 rounded-full bg-blue-600 px-6 py-1 text-sm text-white">
                        <span>Tiếp Quản</span>
                    </Button>
                </DialogTrigger>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>Xác Nhận Tiếp Quản</DialogTitle>
                        <DialogDescription>
                            Bạn có chắc chắn muốn tiếp quản không? Công việc
                            hiện tại của tác giả khác sẽ được lưu dưới dạng
                            nháp.
                        </DialogDescription>
                    </DialogHeader>
                    <DialogFooter className="gap-2 sm:justify-end">
                        <DialogClose asChild>
                            <Button type="button" variant="ghost">
                                Hủy
                            </Button>
                        </DialogClose>
                        <DialogClose asChild>
                            <Button type="button" variant="default">
                                Tiếp Quản
                            </Button>
                        </DialogClose>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
}
