"use client";
import React, { ReactNode, useEffect, useMemo, useState } from "react";
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
import { ComboBox } from "@/components/ComboBox";
import { IoEllipsisVerticalOutline } from "react-icons/io5";
import cx from "classnames";
import { useDebounceValue } from "usehooks-ts";
import { getChapters } from "@/utils/api/author-studio/volume";
import { toast } from "sonner";
import UwU from "./UwU";
import { RiListOrdered2 } from "react-icons/ri";
import { reorderChapter } from "@/utils/api/author-studio/chapter";

const testData = [
    { value: "1", label: "One", chapterNumber: undefined },
    { value: "2", label: "Two", chapterNumber: 2 },
    { value: "3", label: "Three", chapterNumber: 3 },
];

export interface ReorderDialogProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;
    children?: ReactNode;
    onReordered?: (newOrder: number) => void;
}

export default function ReorderDialog(props: ReorderDialogProps) {
    const { seriesId, volumeId, chapterId, children, onReordered } = props;
    const [isOpen, setIsOpen] = useState(false);
    const [options, setOptions] = useState(testData);
    const [search, setSearch] = useDebounceValue("", 500);
    const [selected, setSelected] = useState<string>();
    const [selectedInfo, setSelectedInfo] = useState<any>();
    const [newPosition, setNewPosition] = useState<number>();
    const [currentPos, setCurrentPos] = useState<number>();

    useEffect(() => {
        async function work() {
            const [data, err] = await getChapters(
                seriesId,
                volumeId,
                1,
                50,
                search,
            );
            if (err) {
                console.error(err);
                toast.error("Tải chương thất bại");
                return;
            }

            setCurrentPos(
                data.items.find((x) => x.id === chapterId)?.chapterNumber,
            );

            const processed = [
                selectedInfo?.value === "0" ? undefined : selectedInfo,
                {
                    value: "0",
                    label: "Đặt làm chương đầu tiên!",
                    chapterNumber: 0,
                },
                ...data.items
                    .map((x) => ({
                        value: x.id.toString(),
                        label: `Ch. ${x.chapterNumber}: ${x.title}`,
                        chapterNumber: x.chapterNumber,
                    }))
                    .filter((x) => x.value !== chapterId),
            ]
                .filter((x) => x !== undefined)
                .filter((x) => x.value !== chapterId);

            setOptions(processed);
        }

        work();
    }, [search, seriesId, volumeId, chapterId, selectedInfo]);

    async function handleReorder() {
        if (selected === undefined || selected === null) {
            toast.error("Vui lòng chọn chương để đặt phía sau!");
            return;
        }

        const [data, err] = await reorderChapter(
            seriesId,
            volumeId,
            chapterId,
            selected,
        );

        if (err) {
            console.error(err);
            toast.error("Thất bại khi sắp xếp lại chương");
            return;
        }

        toast.success("Chương đã được sắp xếp lại!");
        setIsOpen(false);
        onReordered?.(newPosition ?? 0);
    }

    return (
        <Dialog open={isOpen} onOpenChange={(x) => setIsOpen(x)}>
            <DialogTrigger asChild>{children}</DialogTrigger>
            <DialogContent className="max-h-[90dvh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Sắp xếp lại chương</DialogTitle>
                </DialogHeader>
                <DialogDescription>
                    Việc sắp xếp lại chương sẽ đặt chương sau chương được chọn
                    bên dưới.
                </DialogDescription>
                <div>
                    <p>Chương trước</p>
                    <ComboBox
                        data={options}
                        onChange={(e) => {
                            setSelected(e);
                            setSelectedInfo(options.find((x) => x.value === e));
                            setNewPosition(
                                e === "first"
                                    ? 0
                                    : options.find((x) => x.value === e)
                                          ?.chapterNumber,
                            );
                        }}
                        onSearch={(e) => setSearch((e.target as any).value)}
                        value={selected}
                        placeholder="Chọn chương"
                        searchPlaceHolder="Tìm kiếm tiêu đề hoặc số chương..."
                    />

                    <UwU>
                        <p className="mt-4">Xem trước sắp xếp lại</p>
                        <ReorderPreview
                            seriesId={seriesId}
                            volumeId={volumeId}
                            chapterId={chapterId}
                            currentPosition={currentPos ?? 10}
                            newPosition={40}
                        />
                    </UwU>
                </div>
                <DialogFooter className="gap-2 sm:justify-end">
                    <DialogClose asChild>
                        <Button type="button" variant="ghost">
                            Hủy
                        </Button>
                    </DialogClose>
                    <Button
                        className="flex gap-2 bg-blue-600 text-white hover:bg-blue-700"
                        onClick={handleReorder}
                    >
                        <RiListOrdered2 />
                        Sắp xếp lại chương!
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

export interface ReorderPreviewProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;
    currentPosition: number;
    newPosition?: number;
}

function ReorderPreview(props: ReorderPreviewProps) {
    const { seriesId, volumeId, chapterId, newPosition, currentPosition } =
        props;

    // const [testData, setTestData] = useState<any[]>([]);

    // useEffect(() => {
    //     async function work() {
    //         const reqRange1Low = Math.max(1, currentPosition - 2);
    //         const reqRange1High = currentPosition + 2;
    //         const reqRange1Count = reqRange1High - reqRange1Low + 1;

    //         const [dataLow, errLow] = await getChapters(
    //             seriesId,
    //             volumeId,
    //             1,
    //             reqRange1Count,
    //         );

    //         if (errLow) {
    //             console.error(errLow);
    //             toast.error("Failed to load chapters");
    //             return;
    //         }

    //         const reqRange2Low = Math.max(
    //             1,
    //             (newPosition ?? currentPosition) - 2,
    //         );
    //         const reqRange2High = (newPosition ?? currentPosition) + 2;

    //         const [data, err] = await getChapters(seriesId, volumeId, 1, 50);

    //         if (err) {
    //             console.error(err);
    //             toast.error("Failed to load chapters");
    //             return;
    //         }

    //         // setTestData(data.items);
    //     }
    // }, []);

    const testData = Array.from({ length: 100 }, (_, i) => ({
        id: i.toString(),
        chapterNumber: i + 1,
        title: `Chapter ${i + 1} `,
    }));

    const curOrder = testData.find((x) => x.id === chapterId)!;
    const number = curOrder.chapterNumber;
    const items = useMemo(() => {
        let variant = "";
        let shown: (number | "")[] = [
            number - 2,
            number - 1,
            number,
            number + 1,
            number + 2,
        ];

        if (newPosition !== undefined) {
            const shownNew = [
                newPosition - 2,
                newPosition - 1,
                newPosition,
                newPosition + 1,
                newPosition + 2,
            ];

            const shownSet = new Set(shown);
            shownNew.forEach((x) => shownSet.add(x));

            if (shown.length + shownNew.length === shownSet.size) {
                if (number < newPosition) {
                    shown.push("", ...shownNew);
                } else {
                    shown.unshift(...shownNew, "");
                }
            } else {
                shown = (Array.from(shownSet) as number[]).sort(
                    (a, b) => a - b,
                );
            }
        }

        shown.unshift("");
        shown.push("");
        return shown
            .map((x) =>
                x === "" ? "" : testData.find((y) => y.chapterNumber === x),
            )
            .filter((x) => x !== undefined);
    }, [newPosition, number, testData]);

    return (
        <ul className="mix-blend-hard-light">
            {items.map((x, i) =>
                x === "" ? (
                    <IoEllipsisVerticalOutline
                        key={i}
                        className="mx-auto opacity-30"
                        size={50}
                    />
                ) : (
                    <>
                        {x.chapterNumber === newPosition &&
                            number > newPosition && (
                                <PreviewItem
                                    key={curOrder.id + "new"}
                                    state={"added"}
                                >
                                    Ch. {newPosition}: {curOrder.title}
                                </PreviewItem>
                            )}
                        <PreviewItem
                            key={x.id}
                            state={
                                x.chapterNumber === number
                                    ? "removed"
                                    : "normal"
                            }
                        >
                            Ch.{" "}
                            {findNewPosition(
                                x.chapterNumber,
                                number,
                                newPosition,
                            )}
                            : {x.title}
                        </PreviewItem>
                        {x.chapterNumber === newPosition &&
                            number < newPosition && (
                                <PreviewItem
                                    key={curOrder.id + "new"}
                                    state={"added"}
                                >
                                    Ch. {newPosition}: {curOrder.title}
                                </PreviewItem>
                            )}
                    </>
                ),
            )}
        </ul>
    );
}

export interface PreviewItemProps {
    children: ReactNode;
    state: "normal" | "removed" | "added";
}

function findNewPosition(current: number, removed: number, added?: number) {
    let x = current;
    if (current > removed) {
        x--;
    }

    if (current > (added ?? Number.MAX_SAFE_INTEGER)) {
        x++;
    }

    if (current === added && added < removed) {
        x++;
    }

    return x;
}

function PreviewItem({ children, state }: PreviewItemProps) {
    return (
        <li
            className={cx("border-b border-t bg-opacity-20 p-2", {
                "bg-red-600": state === "removed",
                "bg-green-600": state === "added",
                "opacity-30": state === "normal",
                "line-through": state === "removed",
            })}
        >
            {children}
        </li>
    );
}
