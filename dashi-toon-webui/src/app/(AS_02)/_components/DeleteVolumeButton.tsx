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
import { Button } from "@/components/ui/button";
import { FaTrash } from "react-icons/fa";
import { useRouter } from "next/navigation";
import { removeVolume } from "@/utils/api/author-studio/volume";
import { toast } from "sonner";
import { LoadingSpinner } from "@/components/ui/spinner";
import cx from "classnames";

export interface DeleteVolumeButtonProps {
    seriesId: string;
    volumeId: string;
}

export default function DeleteVolumeButton(props: DeleteVolumeButtonProps) {
    const { seriesId, volumeId } = props;
    const router = useRouter();
    const [isDeleting, setIsDeleting] = useState(false);
    const handleDelete = async () => {
        setIsDeleting(true);
        const [_, err] = await removeVolume(props.seriesId, props.volumeId);

        setIsDeleting(false);
        if (err) {
            toast.error("Không xóa được tập truyện");
            return;
        }

        toast.success("Đã xóa thành công tập truyện");
        router.push(`/author-studio/series/${seriesId}`);
    };

    return (
        <Dialog>
            <DialogTrigger asChild>
                <Button className="mt-4" variant={"destructive"} type="button">
                    <FaTrash className="mr-2" /> Xóa Tập Truyện
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Xác nhận xóa tập truyện</DialogTitle>
                    <DialogDescription>
                        Bạn có chắc chắn muốn xóa tập hiện tại không? Hành động
                        này không thể hoàn tác. Bất kỳ chương nào liên quan đến
                        tập này sẽ bị xóa.
                    </DialogDescription>
                </DialogHeader>
                <DialogFooter className="gap-2 sm:justify-end">
                    <DialogClose asChild>
                        <Button type="button" variant="ghost">
                            Hủy bỏ
                        </Button>
                    </DialogClose>
                    <Button
                        variant={"destructive"}
                        onClick={handleDelete}
                        className="flex gap-2"
                    >
                        {isDeleting ? (
                            <LoadingSpinner
                                className={cx({ hidden: !isDeleting })}
                            />
                        ) : (
                            <FaTrash />
                        )}
                        Xóa Tập Truyện!
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
