import React from "react";
import EditChapterScreen from "@/app/(AS_03)/_screens/EditChapterScreen";
import { NextPageProps } from "@/utils/nextjs";

export default function AS_03_Page(props: NextPageProps) {
    const { searchParams, params } = props;
    const { id, vid, cid } = params;
    return <EditChapterScreen chapterId={cid} seriesId={id} volumeId={vid} />;
}
