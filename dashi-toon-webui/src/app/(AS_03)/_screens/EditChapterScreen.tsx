"use client";

import Link from "next/link";
import React, { useEffect, useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import DeleteChapterButton from "../_components/DeleteChapterButton";

import {
    Breadcrumb,
    BreadcrumbEllipsis,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import Rte from "../_components/Rte";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Textarea } from "@/components/ui/textarea";
import { Separator } from "@/components/ui/separator";
import ThumbnailUpload from "../_components/ThumbnailUpload";
import { FaSave, FaEye, FaTrash, FaCalendar } from "react-icons/fa";
import { FaGear } from "react-icons/fa6";
import { IoSend } from "react-icons/io5";
import {
    editChapterComic,
    editChapterNovel,
    getChapterContentComic,
    getChapterContentNovel,
    publishChapter,
    setChapterPricing,
    unpublishChapter,
} from "@/utils/api/author-studio/chapter";
import { toast } from "sonner";
import { ChapterItem, getChapters, getPrevChapter, getVolumeInfo } from "@/utils/api/author-studio/volume";
import { getSeriesInfo, SeriesInfo } from "@/utils/api/author-studio/series";
import ComicImgUpload from "../_components/ComicImgUpload";
import { cx } from "class-variance-authority";
import ReorderDialog from "@/components/ReorderDialog";
import { RiListOrdered2 } from "react-icons/ri";
import EditChapterHistory from "../_components/EditChapterHistory";
import { useDebounceCallback } from "usehooks-ts";
import PreviewButton from "@/app/(Reader)/components/PreviewButton";
import { Home, Save } from "lucide-react";
import SiteLayout from "@/components/SiteLayout";
import { DatePicker } from "@/components/ui/date-picker";
import { DateTimePicker } from "@/components/ui/date-time-picker";
import { vi } from "date-fns/locale/vi";
import { MdMoneyOff } from "react-icons/md";
import PublishOutOfOrderDialog from "../_components/PublishOutOfOrderDialog";

const formSchema = z.object({
    title: z.string().max(256, {
        message: "Title must be less than 256 characters.",
    }),

    thumbnail: z.string().optional().nullable(),
    content: z.union([z.string(), z.any()]),
    note: z.string().max(2000, { message: "Notes must be less than 2000 characters." }).optional().nullable(),
    timestamp: z.any(),
    versionId: z.any(),
});

const AUTOSAVE_INTERVAL = 1000 * 60 * 5;

export default function EditChapterScreen(props: EditChapterScreenProps) {
    const { seriesId, volumeId, chapterId } = props;

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            title: "Chương chưa có tiêu đề",
            note: "",
            content: [],
        },
    });

    const lastAutoSave = useRef<number>(Date.now());
    const [series, setSeries] = useState<SeriesInfo | null>(null);
    const [volumeName, setVolumeName] = useState<string | null>(null);
    const [volumeNo, setVolumeNo] = useState(1);
    const [publishedDate, setPublishedDate] = useState<Date | null>(null);
    const [newPublishedDate, setNewPublishedDate] = useState<Date>();
    const [price, setPrice] = useState<number>(0);
    const [chapterNumber, setChapterNumber] = useState<number>(0);
    const [po3Open, setPo3Open] = useState(false);
    const [prevO3Chapter, setPrevO3Chapter] = useState<ChapterItem | null>(null);
    const [o3PublishNow, setO3PublishNow] = useState<boolean>(false);
    const handleChanges = useDebounceCallback(() => {
        const now = Date.now();
        if (now - lastAutoSave.current > AUTOSAVE_INTERVAL) {
            lastAutoSave.current = now;
            form.handleSubmit((v) => onSubmit(v, true))();
        }
    }, 5000);

    useEffect(() => {
        async function work() {
            const [seriesData, seriesErr] = await getSeriesInfo(seriesId);
            if (seriesErr) {
                toast.error("Không tải được nội dung chương. Vui lòng tải lại trang.");

                return;
            }

            if (seriesData) {
                setSeries(seriesData);
                console.log(seriesData);
            }

            const [volumeData, volumeErr] = await getVolumeInfo(seriesId, volumeId);

            if (volumeErr) {
                toast.error("Không tải được nội dung chương. Vui lòng tải lại trang.");

                return; //asd
            }

            if (volumeData) {
                setVolumeName(volumeData.name);
                setVolumeNo(volumeData.volumeNumber);
            }

            const [data, err] =
                seriesData.type.toString() === "1"
                    ? await getChapterContentNovel(seriesId, volumeId, chapterId)
                    : await getChapterContentComic(seriesId, volumeId, chapterId);

            if (err) {
                toast.error("Không tải được nội dung chương. Vui lòng tải lại trang.");

                return;
            }

            if (seriesData.type.toString() === "2") {
                data.content = data.content.map((x: any) => ({
                    id: x.imageName,
                    imageUrl: x.imageUrl,
                    imageSize: x.imageSize,
                }));
            }

            if (data) {
                form.reset(data);
                if (data.publishedDate) setPublishedDate(new Date(data.publishedDate));
            }

            setPrice(data.price ?? 0);
        }

        work();
    }, [seriesId, volumeId, chapterId, form]);

    useEffect(() => {
        const sub = form.watch((x) => handleChanges());
        return sub.unsubscribe;
    }, [form, handleChanges]);

    async function onSubmit(values: z.infer<typeof formSchema>, isAutoSave?: boolean) {
        console.log({ values, isAutoSave });
        if (series?.type.toString() === "1") {
            const [_, err] = await editChapterNovel(seriesId, volumeId, chapterId, {
                ...values,
                isAutoSave: isAutoSave ?? false,
            });

            if (err) {
                toast.error("Không cập nhật được chương");
            }
        } else {
            const [_, err] = await editChapterComic(seriesId, volumeId, chapterId, {
                ...values,
                content: values.content.map((x: any) => x.id),
                isAutoSave: isAutoSave ?? false,
            });
            if (err) {
                toast.error("Không cập nhật được chương");
            }
        }

        const [data, err] =
            series?.type === 1
                ? await getChapterContentNovel(seriesId, volumeId, chapterId)
                : await getChapterContentComic(seriesId, volumeId, chapterId);

        form.setValue("versionId", data.versionId);

        if (!isAutoSave) {
            toast.success("Chương đã được cập nhật thành công");
        }
    }

    async function handleUnpublish() {
        const [_, err] = await unpublishChapter(seriesId, volumeId, chapterId);
        if (err) {
            toast.error("Không thể hủy xuất bản chương");
            return;
        }

        toast.success("Chương đã được hủy xuất bản");
        setPublishedDate(null);
    }

    async function handlePublish(isNow: boolean, publishTime?: Date, checkPrev = true) {
        if (!isNow) {
            if (publishTime === null || publishTime === undefined) {
                toast.error("Vui lòng chọn ngày xuất bản!");
                return;
            }
        }

        const actual = isNow ? new Date() : publishTime!;

        if (checkPrev) {
            const [prev, _1] = await getPrevChapter(seriesId, volumeId, chapterId);
            if (prev && (!prev.publishedDate || new Date(prev.publishedDate) > actual)) {
                setPrevO3Chapter(prev);
                setPo3Open(true);
                setO3PublishNow(isNow);
                return;
            }
        }
        await onSubmit(form.getValues(), false);

        const [_, err] = await publishChapter(seriesId, volumeId, chapterId, isNow ? null : actual);

        if (err) {
            toast.error("Không thể xuất bản chương");
            return;
        }

        toast.success("Chương đã xuất bản thành công");
        setPublishedDate(actual);
        setNewPublishedDate(actual);
    }

    async function handleChangePrice(isFree: boolean) {
        const [_, err] = await setChapterPricing({
            seriesId: Number.parseInt(seriesId),
            volumeId: Number.parseInt(volumeId),
            chapterId: Number.parseInt(chapterId),
            price: isFree ? null : price === 0 ? null : price,
        });

        if (err) {
            toast.error("Không thể cập nhật giá chương");
            return;
        }

        toast.success("Giá chương đã được cập nhật");
    }

    return (
        <SiteLayout>
            <div className="justify-center pb-4">
                <Form {...form}>
                    <form onSubmit={form.handleSubmit((v) => onSubmit(v))} className="container flex gap-4 px-2">
                        <div className="hidden w-60 flex-shrink-0 lg:flex">
                            <FormField
                                name="thumbnail"
                                control={form.control}
                                render={({ field }) => (
                                    <FormItem className="my-4">
                                        <FormLabel>Hình thu nhỏ</FormLabel>
                                        <FormControl>
                                            <ThumbnailUpload
                                                containerClassName="max-w-96 w-full mx-auto"
                                                fileName={field.value!}
                                                onFilenameChanged={(file) => {
                                                    form.setValue("thumbnail", file);
                                                }}
                                            />
                                        </FormControl>
                                        <FormDescription>
                                            Tỷ lệ khung hình được đề xuất là 3/4. Hình ảnh phải nhỏ hơn 1MB. Chỉ hỗ trợ
                                            JPG, JPEG và PNG
                                        </FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>

                        <div className="flex-shrink flex-grow">
                            <Breadcrumb className="my-4">
                                <BreadcrumbList>
                                    <BreadcrumbItem>
                                        <BreadcrumbLink href="/" className="flex items-center">
                                            <Home className="mr-2 h-4 w-4" />
                                        </BreadcrumbLink>
                                    </BreadcrumbItem>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbLink href="/author-studio">Xưởng Truyện</BreadcrumbLink>
                                    </BreadcrumbItem>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbLink href="/author-studio/series">Bộ truyện</BreadcrumbLink>
                                    </BreadcrumbItem>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbLink href={`/author-studio/series/${seriesId}`}>
                                            {series?.title ?? "Đang tải"}
                                        </BreadcrumbLink>
                                    </BreadcrumbItem>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbLink href={`/author-studio/series/${seriesId}/vol/${volumeId}`}>
                                            Tập {volumeNo ?? 1}: {volumeName ?? "Đang tải..."}
                                        </BreadcrumbLink>
                                    </BreadcrumbItem>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbPage>Chỉnh sửa chương</BreadcrumbPage>
                                    </BreadcrumbItem>
                                </BreadcrumbList>
                            </Breadcrumb>
                            <h1 className="text-2xl">Tiêu đề truyện: {series?.title ?? "Đang tải..."}</h1>
                            <h2 className="flex items-center gap-2 text-lg">
                                Tập {volumeNo}: {volumeName}
                                <ReorderDialog chapterId={chapterId} seriesId={seriesId} volumeId={volumeId}>
                                    <Button variant={"outline"} className="flex h-8 gap-2 underline">
                                        <RiListOrdered2 /> Sắp xếp
                                    </Button>
                                </ReorderDialog>
                            </h2>

                            <FormField
                                name="title"
                                control={form.control}
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Tiêu đề chương</FormLabel>
                                        <FormControl>
                                            <Input placeholder="Enter your chapter title here..." {...field} />
                                        </FormControl>
                                        <FormDescription>Ít hơn 256 ký tự</FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <FormField
                                name="thumbnail"
                                control={form.control}
                                render={({ field }) => (
                                    <FormItem className="mt-4 lg:hidden">
                                        <FormLabel>Hình thu nhỏ</FormLabel>
                                        <FormControl>
                                            <ThumbnailUpload
                                                containerClassName="max-w-96 w-full mx-auto"
                                                fileName={field.value!}
                                                onFilenameChanged={(file) => {
                                                    form.setValue("thumbnail", file);
                                                }}
                                            />
                                        </FormControl>
                                        <FormDescription>
                                            Tỷ lệ khung hình được đề xuất là 3/4. Hình ảnh phải nhỏ hơn 1MB. Chỉ hỗ trợ
                                            JPG, JPEG và PNG
                                        </FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            {series?.type === 2 ? (
                                <FormField
                                    name="content"
                                    control={form.control}
                                    render={({ field }) => (
                                        <FormItem className="mt-4 flex flex-col">
                                            <FormLabel>Nội dung</FormLabel>
                                            <FormControl>
                                                <ComicImgUpload
                                                    value={field.value}
                                                    onChange={(v) => form.setValue("content", v)}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Định dạng tệp được hỗ trợ: JPG, JPEG, PNG.
                                                <br />
                                                Kích thước tải lên tối đa: 20MB
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            ) : (
                                <FormField
                                    name="content"
                                    control={form.control}
                                    render={({ field }) => (
                                        <FormItem className="mt-4 flex flex-col">
                                            <FormLabel>Nội dung</FormLabel>
                                            <FormControl>
                                                <Rte
                                                    content={field.value}
                                                    onChange={(e) => form.setValue("content", e)}
                                                />
                                            </FormControl>
                                            <FormDescription>
                                                Định dạng tệp được hỗ trợ: JPG, JPEG, PNG.
                                                <br />
                                                Kích thước tải lên tối đa: 20MB
                                            </FormDescription>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                            )}

                            <FormField
                                name="note"
                                control={form.control}
                                render={({ field }) => (
                                    <FormItem className="mt-4">
                                        <FormLabel>Ghi chú của tác giả (Tùy chọn)</FormLabel>
                                        <FormControl>
                                            <Textarea
                                                placeholder="Nhập mô tả..."
                                                value={field.value ?? ""}
                                                onChange={field.onChange}
                                            />
                                        </FormControl>
                                        <FormDescription>Chiều dài tối đa 2000 ký tự</FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <div className="mt-4 flex flex-wrap gap-2">
                                <Button
                                    type="button"
                                    onClick={() => handlePublish(true)}
                                    className={cx("flex gap-2 bg-blue-600 text-white", {
                                        hidden: publishedDate === null,
                                    })}
                                >
                                    <IoSend /> Lưu và xuất bản ngay
                                </Button>
                                <Button className="flex gap-2 bg-neutral-700 text-white">
                                    <FaSave /> Lưu bản nháp
                                </Button>

                                <PreviewButton
                                    seriesId={seriesId}
                                    type={series?.type === 1 ? "novel" : "comic"}
                                    volumeId={volumeId}
                                    chapterId={chapterId}
                                    versionId={form.getValues("versionId")}
                                />
                                <DeleteChapterButton chapterId={chapterId} volumeId={volumeId} seriesId={seriesId} />
                            </div>

                            <p className="pt-2 text-sm text-muted-foreground">
                                Lần cuối lưu tại{" "}
                                {form.getValues("timestamp")
                                    ? new Date(form.getValues("timestamp")).toLocaleString()
                                    : "Unknown"}{" "}
                                <EditChapterHistory chapterId={chapterId} volumeId={volumeId} seriesId={seriesId}>
                                    <span className="underline">(Xem lịch sử)</span>
                                </EditChapterHistory>
                            </p>

                            <Separator className="my-4 bg-neutral-600" />

                            <h2 className="text-lg">Xuất bản</h2>
                            <p className="text-muted-foreground">
                                Sau khi xuất bản, chương này sẽ được ra mắt sớm với những độc giả đăng ký DashiFan ngay
                                lập tức và sẽ ra mắt chính thức vào ngày được chọn. Bạn có thể đổi ý bằng cách hủy xuất
                                bản
                            </p>
                            {
                                <p>
                                    {publishedDate
                                        ? `Chương này xuất bản vào ${publishedDate.toLocaleString()}`
                                        : "Chương này chưa được xuất bản"}
                                </p>
                            }

                            <div className="mt-4">
                                <FormLabel>Chọn ngày xuất bản</FormLabel>
                                <div className="flex gap-2">
                                    <FormControl>
                                        <DateTimePicker
                                            disabled={publishedDate !== null}
                                            locale={vi}
                                            value={newPublishedDate}
                                            onChange={(e) => {
                                                const today = new Date();
                                                today.setHours(0, 0, 0, 0);
                                                if (e && today > e) {
                                                    toast.error("Không thể chọn ngày xuất bản trong quá khứ");
                                                    return;
                                                }
                                                setNewPublishedDate(e);
                                            }}
                                        />
                                    </FormControl>
                                    <Button
                                        className={cx("flex gap-2 bg-blue-600 text-white", {
                                            hidden: publishedDate !== null,
                                        })}
                                        type="button"
                                        onClick={() => handlePublish(false, newPublishedDate)}
                                    >
                                        <FaCalendar />
                                        Lên lịch xuất bản
                                    </Button>
                                    <Button
                                        className={cx("flex gap-2 bg-neutral-700 text-white", {
                                            hidden: publishedDate !== null,
                                        })}
                                        type="button"
                                        onClick={() => handlePublish(true, new Date())}
                                    >
                                        <IoSend />
                                        Xuất bản ngay lập tức
                                    </Button>

                                    <Button
                                        onClick={handleUnpublish}
                                        className={cx("flex gap-2 bg-neutral-700 text-white", {
                                            hidden: publishedDate === null,
                                        })}
                                        type="button"
                                    >
                                        <FaCalendar />
                                        Hủy xuất bản chương
                                    </Button>
                                    <Button
                                        onClick={() => handlePublish(true)}
                                        className={cx("flex gap-2 bg-blue-600 text-white", {
                                            hidden: publishedDate === null,
                                        })}
                                        type="button"
                                    >
                                        <IoSend />
                                        Xuất bản lại ngay
                                    </Button>
                                </div>
                            </div>

                            <Separator className="my-4 bg-neutral-600" />

                            <h2 className="text-lg">Cài đặt giá cho chương truyện</h2>
                            <p className="mb-4 text-muted-foreground">
                                Độc giả không phải là DashiFan có thể đọc chương này miễn phí hoặc phải mua bằng
                                KanaCoin hoặc KanaGold với cài đặt dưới đây:
                            </p>

                            <FormLabel>Giá để mở khóa chương</FormLabel>
                            <div className="flex gap-2">
                                <FormControl>
                                    <Input
                                        type="number"
                                        placeholder="Giá để mở khóa chương"
                                        value={price}
                                        min={0}
                                        onChange={(e) => setPrice(Number.parseFloat(e.target.value))}
                                    />
                                </FormControl>
                                <Button
                                    className="flex gap-2 bg-blue-600 text-white"
                                    type="button"
                                    onClick={() => handleChangePrice(false)}
                                >
                                    <Save /> Lưu thay đổi
                                </Button>
                                <Button
                                    className="flex gap-2 bg-neutral-700 text-white"
                                    type="button"
                                    onClick={() => handleChangePrice(true)}
                                >
                                    <MdMoneyOff size={24} /> Cho phép đọc miễn phí
                                </Button>
                            </div>
                        </div>
                    </form>
                </Form>

                <PublishOutOfOrderDialog
                    chapterId={chapterId}
                    seriesId={seriesId}
                    volumeId={volumeId}
                    prevChapter={prevO3Chapter ?? undefined}
                    isOpen={po3Open}
                    setIsOpen={(e) => setPo3Open(e)}
                    handlePublish={async () => {
                        await handlePublish(o3PublishNow, newPublishedDate, false);
                        setPo3Open(false);
                    }}
                />
            </div>
        </SiteLayout>
    );
}

export interface EditChapterScreenProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;
}
