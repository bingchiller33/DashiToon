import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import ReadNovelScreen from "@/app/(Reader)/_screens/ReadNovel";

export default function ReadNovel_Page(props: NextPageProps) {
    const { params } = props;
    const { id, vid, cid } = params;

    return <ReadNovelScreen seriesId={id} volumeId={vid} chapterId={cid} />;
}
