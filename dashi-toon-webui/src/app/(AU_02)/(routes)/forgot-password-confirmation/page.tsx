import React from "react";
import AU_02_01bScreen from "../../_screens/AU_02_01b";
import { NextPageProps } from "../../../../utils/nextjs";

export default function AU_02_01b_Page(props: NextPageProps) {
  const { searchParams } = props;
  return (
    <AU_02_01bScreen test={false} returnPath={searchParams.returnUrl ?? "/"} />
  );
}
