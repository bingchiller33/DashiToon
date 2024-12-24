import React from "react";
import AU_03_01Screen from "../../_screens/AU_03_01";
import { NextPageProps } from "@/utils/nextjs";

export default function AU_03_Page(props: NextPageProps) {
  const { searchParams } = props;
  return <AU_03_01Screen test={false} returnPath={searchParams.returnUrl} />;
}
