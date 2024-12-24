"use client";

import Image from "next/image";
import Link from "next/link";
import React, { useEffect, useState } from "react";

import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import { PiCoinVertical } from "react-icons/pi";
import {
    Chapter,
    getChapters,
    getUserAccessibleChapters,
    getVolumes,
    Volume,
    VolumeChapters,
    VolumeWithChapters,
} from "@/utils/api/series";
import { toast } from "sonner";
import { formatDate } from "date-fns";
import { CoinsIcon } from "lucide-react";

export interface TableOfContentProps {
    seriesId: string;
    seriesType: string;
}

export default function TableOfContent(props: TableOfContentProps) {
    const { seriesId, seriesType } = props;
    const [vc, setVc] = useState<VolumeChapters>();

    useEffect(() => {
        async function work() {
            const [vc, err2] = await getUserAccessibleChapters(seriesId);
            if (err2) {
                toast.error("Không thể tải được tập truyện");
                return;
            }

            setVc(vc);
        }

        work();
    }, [seriesId]);

    return (
        <Accordion type="multiple">
            {vc?.data.map((v) => (
                <VolumeListItem
                    key={v.volumeId}
                    volume={v}
                    seriesType={seriesType}
                    seriesId={seriesId}
                />
            ))}
        </Accordion>
    );
}

interface VolumeListItemProps {
    seriesId: string;
    volume: VolumeWithChapters;
    seriesType: string;
}

function VolumeListItem(props: VolumeListItemProps) {
    const { volume, seriesType, seriesId } = props;

    return (
        <AccordionItem value={volume.volumeId.toString()}>
            <AccordionTrigger>
                Tập {volume.volumeNumber}: {volume.name}
            </AccordionTrigger>
            <AccordionContent>
                <ul className="grid gap-x-4 lg:grid-cols-2">
                    {volume.chapters.map((c) => (
                        <ChapterListItem
                            key={c.id}
                            chapter={c}
                            url={`/series/${seriesId}/vol/${volume.volumeId}/chap/${c.id}/${seriesType}`}
                        />
                    ))}
                </ul>
            </AccordionContent>
        </AccordionItem>
    );
}

interface ChapterListItemProps {
    chapter: Chapter;
    url: string;
}

function ChapterListItem(props: ChapterListItemProps) {
    const { chapter, url } = props;
    return (
        <Link href={url}>
            <li className="flex gap-2 border-t py-2 hover:bg-neutral-600">
                <Image
                    src={chapter.thumbnail}
                    width={60}
                    height={80}
                    alt="Chapter Thumbnail"
                    className="ml-2 object-cover"
                />
                <div className="flex flex-grow flex-col">
                    <p className="text-md">
                        Chương {chapter.chapterNumber}: {chapter.title}
                    </p>
                    <p className="text-sm text-neutral-400">
                        {formatDate(chapter.publishedDate, "dd/MM/yyyy")}
                    </p>

                    <p className="mt-auto flex items-center gap-1 text-xl">
                        <CoinsIcon />{" "}
                        {chapter.price > 0 ? chapter.price : "Miễn phí"}
                    </p>
                </div>
            </li>
        </Link>
    );
}
