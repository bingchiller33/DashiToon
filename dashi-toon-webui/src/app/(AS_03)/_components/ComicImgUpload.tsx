import React, { useEffect, useMemo } from "react";
import { FaTrash, FaUpload } from "react-icons/fa";
import cx from "classnames";
import { uploadThumbnail, getImage } from "@/utils/api/image";
import { toast } from "sonner";
import { useState } from "react";
import Image from "next/image";
import { Button } from "@/components/ui/button";
import { ReactSortable } from "react-sortablejs";
import { uploadChapterImage } from "@/utils/api/author-studio/chapter";
import { openFilePicker } from "@/utils";
import { Skeleton } from "@/components/ui/skeleton";

export interface ComicImgUploadProps {
    value: ImageGallery[];
    onChange: (value: ImageGallery[]) => void;
}

export interface ImageGallery {
    id: string;
    imageUrl?: string;
    imageSize?: number;
    pending?: boolean;
}

export default function ComicImgUpload(props: ComicImgUploadProps) {
    const { value: page, onChange } = props;
    const [isDragging, setIsDragging] = React.useState(false);

    let pp = page;

    async function uploadImage(file: File) {
        const tempId = Math.random().toString();
        pp.push({
            id: tempId,
            pending: true,
        });

        onChange(pp);

        const [data, err] = await uploadChapterImage(file);
        if (err) {
            toast.error(`Tải lên không thành công cho hình ảnh ${file.name}`);
            pp = pp.filter((x) => x.id !== tempId);
            onChange(pp);
            return;
        }

        const [url, err2] = await getImage(data.fileName, "chapters");
        if (err2) {
            toast.error(`Tải lên không thành công cho hình ảnh ${file.name}`);
            pp = pp.filter((x) => x.id !== tempId);
            onChange(pp);
            return;
        }

        const rp = pp.find((x) => x.id === tempId);
        if (rp) {
            rp.id = url.imageName;
            rp.imageUrl = url.imageUrl;
            rp.imageSize = url.imageSize;
            rp.pending = false;
        }
        onChange(pp);
    }

    async function uploadImages(files: File[]) {
        for (const file of files) {
            uploadImage(file);
        }
    }

    async function handleDropFiles(e: React.DragEvent<HTMLDivElement>) {
        e.preventDefault();
        e.stopPropagation();
        setIsDragging(false);
        const files = e.dataTransfer.files;
        uploadImages(Array.from(files));
    }

    function handleDelete(x: string) {
        onChange(page.filter((item) => item.id !== x));
    }

    async function handleUpload() {
        const files = await openFilePicker(undefined, true);
        if (!files) return;
        uploadImages(files);
    }

    const total = page.length === 0 ? 0 : page.map((x) => x.imageSize ?? 0).reduce((a, b) => a + b);
    const totalMb = (total / 1024 / 1024).toFixed(2);

    return (
        <div onDrop={handleDropFiles} onDragEnter={() => setIsDragging(true)} onDragLeave={() => setIsDragging(false)}>
            <div className="flex flex-wrap gap-2 pb-2">
                <Button onClick={handleUpload} type="button" className="flex items-center gap-2 bg-blue-600 text-white">
                    <FaUpload /> Tải lên hình ảnh
                </Button>
                <Button onClick={() => onChange([])} type="button" variant="destructive" className="flex gap-2">
                    <FaTrash /> Xóa tất cả hình ảnh
                </Button>
                <p className="ms-auto text-neutral-500">{totalMb} / 20MB</p>
            </div>
            <ReactSortable
                list={page}
                setList={onChange}
                animation={200}
                className="border-lg relative flex h-[75vh] w-full flex-wrap content-start items-start justify-start gap-2 overflow-clip overflow-y-auto rounded-lg border border-neutral-600 bg-neutral-800 p-2"
            >
                {page?.map?.((item, index) => (
                    <div key={item.id} className="border-lg relative rounded-lg border border-neutral-600">
                        <Image
                            width={200}
                            height={267}
                            src={item.imageUrl ?? "/images/placeholder.png"}
                            alt="test"
                            className={cx("aspect-[3/4] object-contain", {
                                "opacity-0": item.pending,
                            })}
                        />
                        {item.pending && <Skeleton className="absolute inset-0" />}
                        {item.pending ? (
                            <p className="text-center text-sm opacity-70">Đang tải lên...</p>
                        ) : (
                            <p className="text-center text-sm opacity-70">
                                Kích thước tệp: {Math.ceil((item?.imageSize ?? 0) / 1024)} KB
                            </p>
                        )}
                        <Button
                            type="button"
                            onClick={() => handleDelete(item.id)}
                            variant="outline"
                            size="icon"
                            className="absolute right-2 top-2"
                        >
                            <FaTrash color="red" />
                        </Button>
                    </div>
                ))}

                <div
                    onDragEnter={() => setIsDragging(true)}
                    onDragOver={() => setIsDragging(true)}
                    className={cx(
                        "drop pointer-events-none absolute inset-0 flex flex-col items-center justify-center gap-4 bg-[#00000090] opacity-0 transition-all",
                        {
                            "opacity-100": isDragging,
                        },
                    )}
                >
                    <FaUpload className="h-[10vh] w-[10vh]" />
                    <p className="pt-4 text-center">Thả hình ảnh vào đây để tải lên!</p>
                </div>

                <div
                    className={cx("w-full text-center text-neutral-500", {
                        hidden: page.length > 0,
                    })}
                >
                    Không có hình ảnh nào được tải lên
                </div>
            </ReactSortable>
        </div>
    );
}
