import { Button } from "@/components/ui/button";
import { SeriesInfo } from "@/utils/api/author-studio/series";
import Link from "next/link";
import React from "react";
import { FaEye } from "react-icons/fa";

export interface PreviewButtonProps {
    seriesId: string;
    type: string;
    volumeId: string;
    chapterId: string;
    versionId: string;
}

export default function PreviewButton(props: PreviewButtonProps) {
    const { seriesId, volumeId, chapterId, versionId, type } = props;
    const url = `/author-studio/series/${seriesId}/vol/${volumeId}/chap/${chapterId}/preview/${versionId}/${type}`;
    return (
        <Link href={url} target="_blank">
            <Button
                type="button"
                className="flex gap-2 bg-neutral-700 text-white"
            >
                <FaEye /> Xem trước
            </Button>
        </Link>
    );
}
