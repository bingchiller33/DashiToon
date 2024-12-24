import React from "react";
import EditSeriesScreen from "../../../_screens/EditSeries";
import { NextPageProps } from "@/utils/nextjs";

export default function EditSeries_Page(props: NextPageProps) {
  const { params } = props;
  const seriesId = params?.id;
  return <EditSeriesScreen seriesId={seriesId} />;
}
