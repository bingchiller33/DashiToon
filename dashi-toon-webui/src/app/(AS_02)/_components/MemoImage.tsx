import React from "react";

function ImgRaw({ src }: { src: string }) {
    return (
        // eslint-disable-next-line @next/next/no-img-element
        <img
            src={src}
            alt="thumbnail"
            className="h-16 w-10 rounded-md transition-all"
        />
    );
}

const MemoImage = React.memo(ImgRaw);
export default MemoImage;
