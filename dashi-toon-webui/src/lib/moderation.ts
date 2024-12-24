export const formatScore = (score: number): string => {
    const percentage = score * 100;

    if (percentage < 5) return "Rất thấp";
    if (percentage < 20) return "Thấp";
    if (percentage < 50) return "Trung bình";
    if (percentage < 80) return "Cao";
    return "Rất cao";
};

export const getScoreColor = (score: number): string => {
    const percentage = score * 100;

    if (percentage < 5) return "bg-green-100 text-green-800";
    if (percentage < 20) return "bg-blue-100 text-blue-800";
    if (percentage < 50) return "bg-yellow-100 text-yellow-800";
    if (percentage < 80) return "bg-orange-100 text-orange-800";
    return "bg-red-100 text-red-800";
};
