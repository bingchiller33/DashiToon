"use client";

import { useState } from "react";

interface SeriesAboutSectionProps {
    synopsis?: string;
    alternativeTitles?: string[];
}

export default function SeriesAboutSection({ synopsis, alternativeTitles }: SeriesAboutSectionProps) {
    const [isExpanded, setIsExpanded] = useState(false);

    return (
        <div className="space-y-4">
            {alternativeTitles && alternativeTitles.length > 0 && (
                <div>
                    <h3 className="mb-2 font-semibold">Tên khác:</h3>
                    <ul className="space-y-1">
                        {alternativeTitles.map((title, index) => (
                            <li className="list-none" key={index}>
                                {title}
                            </li>
                        ))}
                    </ul>
                </div>
            )}

            {alternativeTitles && alternativeTitles.length > 0 && synopsis && <hr className="border-border" />}

            {synopsis && (
                <div>
                    <h3 className="mb-2 font-semibold">Tóm tắt:</h3>
                    <div className="space-y-2">
                        <p className={`whitespace-pre-line text-justify ${isExpanded ? "" : "line-clamp-4"}`}>
                            {synopsis}
                        </p>
                        <button
                            onClick={() => setIsExpanded(!isExpanded)}
                            className="text-sm text-primary hover:underline"
                        >
                            {isExpanded ? "Thu gọn" : "Xem thêm"}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
