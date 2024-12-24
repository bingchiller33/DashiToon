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
import { ChapterItem } from "@/utils/api/author-studio/volume";
import Link from "next/link";
import { IoSend } from "react-icons/io5";
export interface DeleteChapterButtonProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;

    prevChapter?: ChapterItem;
    isOpen: boolean;
    setIsOpen: (isOpen: boolean) => void;
    handlePublish?: () => Promise<void>;
}

export default function PublishOutOfOrderDialog(
    props: DeleteChapterButtonProps,
) {
    const router = useRouter();
    const [isDeleting, setIsLoading] = useState(false);
    const handleDelete = async () => {
        setIsLoading(true);
        props.handlePublish?.();
        setIsLoading(false);
    };

    const prevLink = `/author-studio/series/${props.seriesId}/vol/${props.volumeId}/chap/${props.prevChapter?.id}`;
    const prevName = props.prevChapter?.title;
    console.log(prevLink, prevName);

    return (
        <Dialog open={props.isOpen} onOpenChange={props.setIsOpen}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>
                        Xác nhận xuất bản không theo thứ tự
                    </DialogTitle>
                    <DialogDescription>
                        Bạn có chắc chắn muốn xuất bản không theo thứ tự? Chương{" "}
                        <Link
                            className="text-blue-600 underline"
                            href={prevLink}
                        >
                            &quot;{prevName}&quot;
                        </Link>{" "}
                        trước đó sẽ xuất bản sau chương hiện tại. Hành động này
                        sẽ khiến độc giả cảm thấy khó hiểu khi theo dõi truyện
                        của bạn.
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
                            <IoSend />
                        )}
                        Xác nhận xuất bản
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
