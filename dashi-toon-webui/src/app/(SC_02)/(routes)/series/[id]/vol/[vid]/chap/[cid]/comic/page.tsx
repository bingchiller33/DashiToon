import React from "react";
import { NextPageProps } from "@/utils/nextjs";
import ReadComicScreen from "@/app/(SC_02)/_screens/ReadComicScreen";

export default function ReadNovel_Page(props: NextPageProps) {
    const { params } = props;
    const { id, vid, cid } = params;

    return <ReadComicScreen chapterId={cid} seriesId={id} volumeId={vid} />;
}
