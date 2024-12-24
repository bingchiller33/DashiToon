import React, { useCallback, useEffect, useMemo, useState } from "react";
import Dropzone, { DropzoneState } from "shadcn-dropzone";
import { FaUpload } from "react-icons/fa";
import cx from "classnames";
import { Skeleton } from "@/components/ui/skeleton";
import { uploadThumbnail, getImage } from "@/utils/api/image";
import { toast } from "sonner";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import Cropper from "react-easy-crop";
import { Button } from "@/components/ui/button";
export interface ThumbnailUploadProps {
    onFilenameChanged: (filename: string) => void;
    fileName?: string;
    containerClassName?: string;
}
export interface CropArea {
    x: number;
    y: number;
    width: number;
    height: number;
}
function createImage(url: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
        const image = new window.Image();
        image.addEventListener("load", () => resolve(image));
        image.addEventListener("error", reject);
        image.src = url;
    });
}

async function getCroppedImg(imageSrc: string, pixelCrop: CropArea): Promise<Blob> {
    const image = await createImage(imageSrc);
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");

    if (!ctx) {
        throw new Error("No 2d context");
    }

    // Set canvas size to desired output size
    canvas.width = pixelCrop.width;
    canvas.height = pixelCrop.height;

    ctx.drawImage(
        image,
        pixelCrop.x,
        pixelCrop.y,
        pixelCrop.width,
        pixelCrop.height,
        0,
        0,
        pixelCrop.width,
        pixelCrop.height,
    );

    return new Promise((resolve) => {
        canvas.toBlob((blob) => {
            if (blob) resolve(blob);
        }, "image/jpeg");
    });
}

export default function ThumbnailUpload(props: ThumbnailUploadProps) {
    const { onFilenameChanged, fileName, containerClassName } = props;
    const [isDragging, setIsDragging] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [preview, setPreview] = useState<string | null>(null);

    const [isCropperOpen, setIsCropperOpen] = useState(false);
    const [crop, setCrop] = useState({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<CropArea | null>(null);
    const [originalFile, setOriginalFile] = useState<File | null>(null);

    const onCropComplete = useCallback((croppedArea: any, croppedAreaPixels: any) => {
        setCroppedAreaPixels(croppedAreaPixels);
    }, []);

    useEffect(() => {
        async function work() {
            if (!fileName) {
                setPreview(null);
                return;
            }
            const [url, err] = await getImage(fileName, "thumbnails");
            if (err) {
                console.error(err);
                toast.error("Không thể tải ảnh thu nhỏ");
                return;
            }
            setPreview(url.imageUrl);
        }

        work();
    }, [fileName]);

    async function onUpload<T extends File>(file: T[]) {
        if (file.length !== 1) {
            return;
        }
        setIsDragging(false);
        setPreview(URL.createObjectURL(file[0]));
        setIsCropperOpen(true);
    }

    async function handleCropConfirm() {
        setIsCropperOpen(false);
        setIsUploading(true);
        const lastImg = preview;
        const blob = await getCroppedImg(preview!, croppedAreaPixels!);
        const croppedFile = new File([blob], "Upload.jpeg", { type: "image/jpeg" });
        setPreview(URL.createObjectURL(croppedFile));

        try {
            const [imgName, err] = await uploadThumbnail(croppedFile);
            if (err) {
                setPreview(lastImg);
                return;
            }
            onFilenameChanged(imgName.fileName);
        } finally {
            setIsUploading(false);
        }
    }

    return (
        <>
            <Dropzone
                maxSize={1000000}
                maxFiles={1}
                showFilesList={false}
                containerClassName={props.containerClassName}
                accept={{
                    "image/*": [".png", ".gif", ".jpeg", ".jpg"],
                }}
                dropZoneClassName="h-[initial]"
                onDropAccepted={onUpload}
                onDragEnter={(e) => setIsDragging(true)}
                onDragLeave={(e) => setIsDragging(false)}
                onDrop={(e) => console.log(e)}
            >
                {(dropzone: DropzoneState) => (
                    <div className="border-lg relative flex aspect-[3/4] w-full justify-center overflow-clip rounded-lg border border-dashed border-neutral-600">
                        {/* eslint-disable-next-line @next/next/no-img-element */}
                        <img
                            src={preview!}
                            alt="Ảnh thu nhỏ của chương"
                            className={cx("object-cover", { hidden: !preview })}
                        />
                        <div
                            className={cx({
                                "opacity-80": isUploading,
                                "opacity-0": !isUploading,
                            })}
                        >
                            <Skeleton
                                className={cx("absolute inset-0 z-10 rounded-xl", {
                                    block: true,
                                })}
                            />
                        </div>
                        <div
                            className={cx(
                                "drop absolute inset-0 z-10 flex flex-col items-center justify-center bg-[#00000090] p-8 opacity-0 transition-all hover:opacity-100",
                                {
                                    "opacity-100": isDragging || (!preview && !isUploading),
                                },
                            )}
                        >
                            <FaUpload className="h-10 w-10" />
                            {dropzone.isDragAccept ? (
                                <p className="pt-4 text-center">Thả ở đây để tải lên!</p>
                            ) : (
                                <p className="pt-4 text-center">Thả hình ảnh ở đây hoặc Nhấn ở đây để chọn hình ảnh</p>
                            )}
                        </div>
                    </div>
                )}
            </Dropzone>
            <Dialog open={isCropperOpen} onOpenChange={setIsCropperOpen}>
                <DialogContent className="sm:max-w-[600px]">
                    <div className="relative h-[400px]">
                        {preview && (
                            <Cropper
                                image={preview}
                                crop={crop}
                                zoom={zoom}
                                aspect={3 / 4}
                                onCropChange={setCrop}
                                onZoomChange={setZoom}
                                onCropComplete={onCropComplete}
                            />
                        )}
                    </div>
                    <div className="mt-4 flex justify-end space-x-2">
                        <Button variant="outline" onClick={() => setIsCropperOpen(false)}>
                            Hủy
                        </Button>
                        <Button onClick={handleCropConfirm}>Cắt & Tải lên</Button>
                    </div>
                </DialogContent>
            </Dialog>
        </>
    );
}
