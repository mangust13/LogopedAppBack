SELECT 
    e.Id AS ExerciseId,
    e.Title,
    e.Description,
    e.VideoPath,
    e.IconName,

    mc.Id AS MainCategoryId,
    mc.Name AS MainCategoryName,
    mc.DisplayName AS MainCategoryDisplayName,
    mc.FolderName,

    t.Id AS TagId,
    t.Name AS TagName,
    t.Category AS TagCategory,
    t.DisplayName AS TagDisplayName

FROM Exercises e

INNER JOIN ExerciseMainCategories mc
    ON e.MainCategoryId = mc.Id

LEFT JOIN ExerciseTagLinks etl
    ON e.Id = etl.ExerciseId

LEFT JOIN ExerciseTags t
    ON etl.TagId = t.Id

ORDER BY e.Id

--SELECT 
--    e.Id,
--    e.Title,
--    mc.DisplayName AS MainCategory,

--    STRING_AGG(t.DisplayName, ', ') AS Tags

--FROM Exercises e
--JOIN ExerciseMainCategories mc ON e.MainCategoryId = mc.Id
--LEFT JOIN ExerciseTagLinks etl ON e.Id = etl.ExerciseId
--LEFT JOIN ExerciseTags t ON etl.TagId = t.Id

--GROUP BY 
--    e.Id,
--    e.Title,
--    mc.DisplayName

--ORDER BY e.Id