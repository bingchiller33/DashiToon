import React from "react";
import AU_02_02Screen from "../../_screens/AU_02_02";
import { NextPageProps } from "../../../../utils/nextjs";

export default function AU_02_02_Page(props: NextPageProps) {
  const { searchParams } = props;
  return (
    <AU_02_02Screen test={false} returnPath={searchParams.returnUrl ?? "/"} />
  );
}
