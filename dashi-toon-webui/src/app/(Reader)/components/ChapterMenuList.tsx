import { useState, useEffect } from "react";
import { SheetContent } from "@/components/ui/sheet";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import Image from "next/image";
import Link from "next/link";
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
import { PiCoinVertical } from "react-icons/pi";
import { formatDate } from "date-fns";
import cx from "classnames";
import { CoinsIcon } from "lucide-react";

interface ChapterMenuListProps {
    seriesId: string;
    volumeId: string;
    chapterId: string;
    type: string;
}

const ChapterMenuList: React.FC<ChapterMenuListProps> = (props: ChapterMenuListProps) => {
    return (
        <SheetContent side="left" className="w-[300px] p-0 sm:w-[400px]">
            <div className="border-b p-3">
                <h2 className="text-2xl font-semibold">Mục lục</h2>
            </div>
            <ScrollArea className="h-[calc(100vh-5rem)]">
                <TableOfContent {...props} />
            </ScrollArea>
        </SheetContent>
    );
};

export default ChapterMenuList;

export function TableOfContent(props: ChapterMenuListProps) {
    const { seriesId, volumeId, chapterId, type } = props;
    const [vc, setVc] = useState<VolumeChapters>();
    useEffect(() => {
        async function work() {
            const [vc, err2] = await getUserAccessibleChapters(seriesId);
            if (err2) {
                console.error("Failed to fetch volumes", err2);
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
                    seriesId={seriesId}
                    volumeId={volumeId}
                    type={type}
                    volume={v}
                    chapterId={chapterId}
                    isCurrent={volumeId === v.volumeId.toString()}
                />
            ))}
        </Accordion>
    );
}

interface VolumeListItemProps extends ChapterMenuListProps {
    volume: VolumeWithChapters;
    isCurrent?: boolean;
}

function VolumeListItem(props: VolumeListItemProps) {
    const { seriesId, volume, isCurrent, chapterId, type } = props;

    return (
        <AccordionItem value={volume.volumeId.toString()}>
            <AccordionTrigger className="px-3">
                <p className={cx("text-left", { "text-blue-500": isCurrent })}>
                    Tập {volume.volumeNumber}: {volume.name}
                </p>
            </AccordionTrigger>
            <AccordionContent>
                <ul className="grid gap-x-4">
                    {volume.chapters.map((c) => (
                        <ChapterListItem
                            key={c.id}
                            chapter={c}
                            seriesId={seriesId}
                            volumeId={volume.volumeId.toString()}
                            chapterId={c.id.toString()}
                            type={type}
                            isCurrent={chapterId === c.id.toString()}
                        />
                    ))}
                </ul>
            </AccordionContent>
        </AccordionItem>
    );
}

interface ChapterListItemProps extends ChapterMenuListProps {
    chapter: Chapter;
    isCurrent?: boolean;
    type: string;
}

function ChapterListItem(props: ChapterListItemProps) {
    const { chapter, isCurrent, type, seriesId, volumeId } = props;
    const url = `/series/${seriesId}/vol/${volumeId}/chap/${chapter.id}/${type}`;

    return (
        <Link href={url}>
            <li className="flex gap-2 border-t py-2 text-left hover:bg-neutral-600">
                <Image
                    src={chapter.thumbnail}
                    width={60}
                    height={80}
                    alt="Chapter Thumbnail"
                    className="ml-2 rounded-sm object-cover"
                />
                <div className="flex flex-grow flex-col">
                    <p
                        className={cx("text-md", {
                            "text-blue-500": isCurrent,
                        })}
                    >
                        Chương {chapter.chapterNumber}: {chapter.title}
                    </p>
                    <p className="text-sm text-neutral-400">{formatDate(chapter.publishedDate, "dd/MM/yyyy")}</p>

                    <p className="mt-auto flex items-center gap-1 text-xl">
                        <CoinsIcon /> {chapter.price > 0 ? chapter.price : "Miễn phí"}
                    </p>
                </div>
            </li>
        </Link>
    );
}
