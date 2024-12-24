import React from "react";
import AU_02_01Screen from "../../_screens/AU_02_01";
import { NextPageProps } from "../../../../utils/nextjs";

export default function AU_02_01_Page(props: NextPageProps) {
  const { searchParams } = props;
  return (
    <AU_02_01Screen test={false} returnPath={searchParams.returnUrl ?? "/"} />
  );
}
