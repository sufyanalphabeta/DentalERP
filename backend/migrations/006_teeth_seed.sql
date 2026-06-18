-- Migration 006: Teeth Reference Table (52 teeth: 32 permanent + 20 primary)
-- FDI Notation

CREATE TABLE IF NOT EXISTS teeth (
    id               SMALLINT     PRIMARY KEY,
    fdi_number       SMALLINT     NOT NULL UNIQUE,
    universal_number SMALLINT,
    name_ar          VARCHAR(50)  NOT NULL,
    name_en          VARCHAR(50)  NOT NULL,
    jaw              VARCHAR(10)  NOT NULL CHECK (jaw IN ('Upper','Lower')),
    side             VARCHAR(10)  NOT NULL CHECK (side IN ('Right','Left')),
    tooth_type       VARCHAR(20)  NOT NULL,
    is_primary       BOOLEAN      NOT NULL DEFAULT FALSE,
    position         SMALLINT     NOT NULL
);

-- Permanent Teeth (Upper Right 11-18)
INSERT INTO teeth (id, fdi_number, universal_number, name_ar, name_en, jaw, side, tooth_type, is_primary, position) VALUES
(11, 11, 8,  'القاطع المركزي العلوي الأيمن', 'Upper Right Central Incisor', 'Upper', 'Right', 'Incisor',  FALSE, 1),
(12, 12, 7,  'القاطع الجانبي العلوي الأيمن', 'Upper Right Lateral Incisor', 'Upper', 'Right', 'Incisor',  FALSE, 2),
(13, 13, 6,  'الناب العلوي الأيمن',           'Upper Right Canine',          'Upper', 'Right', 'Canine',   FALSE, 3),
(14, 14, 5,  'الضاحك الأول العلوي الأيمن',   'Upper Right First Premolar',  'Upper', 'Right', 'Premolar', FALSE, 4),
(15, 15, 4,  'الضاحك الثاني العلوي الأيمن',  'Upper Right Second Premolar', 'Upper', 'Right', 'Premolar', FALSE, 5),
(16, 16, 3,  'الرحى الأولى العلوية اليمنى',  'Upper Right First Molar',     'Upper', 'Right', 'Molar',    FALSE, 6),
(17, 17, 2,  'الرحى الثانية العلوية اليمنى', 'Upper Right Second Molar',    'Upper', 'Right', 'Molar',    FALSE, 7),
(18, 18, 1,  'ضرس العقل العلوي الأيمن',      'Upper Right Third Molar',     'Upper', 'Right', 'Molar',    FALSE, 8),

-- Permanent Teeth (Upper Left 21-28)
(21, 21, 9,  'القاطع المركزي العلوي الأيسر', 'Upper Left Central Incisor',  'Upper', 'Left',  'Incisor',  FALSE, 1),
(22, 22, 10, 'القاطع الجانبي العلوي الأيسر', 'Upper Left Lateral Incisor',  'Upper', 'Left',  'Incisor',  FALSE, 2),
(23, 23, 11, 'الناب العلوي الأيسر',           'Upper Left Canine',           'Upper', 'Left',  'Canine',   FALSE, 3),
(24, 24, 12, 'الضاحك الأول العلوي الأيسر',   'Upper Left First Premolar',   'Upper', 'Left',  'Premolar', FALSE, 4),
(25, 25, 13, 'الضاحك الثاني العلوي الأيسر',  'Upper Left Second Premolar',  'Upper', 'Left',  'Premolar', FALSE, 5),
(26, 26, 14, 'الرحى الأولى العلوية اليسرى',  'Upper Left First Molar',      'Upper', 'Left',  'Molar',    FALSE, 6),
(27, 27, 15, 'الرحى الثانية العلوية اليسرى', 'Upper Left Second Molar',     'Upper', 'Left',  'Molar',    FALSE, 7),
(28, 28, 16, 'ضرس العقل العلوي الأيسر',      'Upper Left Third Molar',      'Upper', 'Left',  'Molar',    FALSE, 8),

-- Permanent Teeth (Lower Left 31-38)
(31, 31, 17, 'القاطع المركزي السفلي الأيسر', 'Lower Left Central Incisor',  'Lower', 'Left',  'Incisor',  FALSE, 1),
(32, 32, 18, 'القاطع الجانبي السفلي الأيسر', 'Lower Left Lateral Incisor',  'Lower', 'Left',  'Incisor',  FALSE, 2),
(33, 33, 19, 'الناب السفلي الأيسر',           'Lower Left Canine',           'Lower', 'Left',  'Canine',   FALSE, 3),
(34, 34, 20, 'الضاحك الأول السفلي الأيسر',   'Lower Left First Premolar',   'Lower', 'Left',  'Premolar', FALSE, 4),
(35, 35, 21, 'الضاحك الثاني السفلي الأيسر',  'Lower Left Second Premolar',  'Lower', 'Left',  'Premolar', FALSE, 5),
(36, 36, 22, 'الرحى الأولى السفلية اليسرى',  'Lower Left First Molar',      'Lower', 'Left',  'Molar',    FALSE, 6),
(37, 37, 23, 'الرحى الثانية السفلية اليسرى', 'Lower Left Second Molar',     'Lower', 'Left',  'Molar',    FALSE, 7),
(38, 38, 24, 'ضرس العقل السفلي الأيسر',      'Lower Left Third Molar',      'Lower', 'Left',  'Molar',    FALSE, 8),

-- Permanent Teeth (Lower Right 41-48)
(41, 41, 25, 'القاطع المركزي السفلي الأيمن', 'Lower Right Central Incisor', 'Lower', 'Right', 'Incisor',  FALSE, 1),
(42, 42, 26, 'القاطع الجانبي السفلي الأيمن', 'Lower Right Lateral Incisor', 'Lower', 'Right', 'Incisor',  FALSE, 2),
(43, 43, 27, 'الناب السفلي الأيمن',           'Lower Right Canine',          'Lower', 'Right', 'Canine',   FALSE, 3),
(44, 44, 28, 'الضاحك الأول السفلي الأيمن',   'Lower Right First Premolar',  'Lower', 'Right', 'Premolar', FALSE, 4),
(45, 45, 29, 'الضاحك الثاني السفلي الأيمن',  'Lower Right Second Premolar', 'Lower', 'Right', 'Premolar', FALSE, 5),
(46, 46, 30, 'الرحى الأولى السفلية اليمنى',  'Lower Right First Molar',     'Lower', 'Right', 'Molar',    FALSE, 6),
(47, 47, 31, 'الرحى الثانية السفلية اليمنى', 'Lower Right Second Molar',    'Lower', 'Right', 'Molar',    FALSE, 7),
(48, 48, 32, 'ضرس العقل السفلي الأيمن',      'Lower Right Third Molar',     'Lower', 'Right', 'Molar',    FALSE, 8),

-- Primary Teeth / Deciduous (51-55 Upper Right)
(51, 51, NULL, 'القاطع المركزي اللبني العلوي الأيمن',  'Upper Right Primary Central Incisor', 'Upper', 'Right', 'Incisor',  TRUE, 1),
(52, 52, NULL, 'القاطع الجانبي اللبني العلوي الأيمن',  'Upper Right Primary Lateral Incisor', 'Upper', 'Right', 'Incisor',  TRUE, 2),
(53, 53, NULL, 'الناب اللبني العلوي الأيمن',             'Upper Right Primary Canine',          'Upper', 'Right', 'Canine',   TRUE, 3),
(54, 54, NULL, 'الضاحك الأول اللبني العلوي الأيمن',    'Upper Right Primary First Molar',     'Upper', 'Right', 'Molar',    TRUE, 4),
(55, 55, NULL, 'الضاحك الثاني اللبني العلوي الأيمن',   'Upper Right Primary Second Molar',    'Upper', 'Right', 'Molar',    TRUE, 5),

-- Primary Teeth (61-65 Upper Left)
(61, 61, NULL, 'القاطع المركزي اللبني العلوي الأيسر',  'Upper Left Primary Central Incisor',  'Upper', 'Left',  'Incisor',  TRUE, 1),
(62, 62, NULL, 'القاطع الجانبي اللبني العلوي الأيسر',  'Upper Left Primary Lateral Incisor',  'Upper', 'Left',  'Incisor',  TRUE, 2),
(63, 63, NULL, 'الناب اللبني العلوي الأيسر',             'Upper Left Primary Canine',           'Upper', 'Left',  'Canine',   TRUE, 3),
(64, 64, NULL, 'الضاحك الأول اللبني العلوي الأيسر',    'Upper Left Primary First Molar',      'Upper', 'Left',  'Molar',    TRUE, 4),
(65, 65, NULL, 'الضاحك الثاني اللبني العلوي الأيسر',   'Upper Left Primary Second Molar',     'Upper', 'Left',  'Molar',    TRUE, 5),

-- Primary Teeth (71-75 Lower Left)
(71, 71, NULL, 'القاطع المركزي اللبني السفلي الأيسر',  'Lower Left Primary Central Incisor',  'Lower', 'Left',  'Incisor',  TRUE, 1),
(72, 72, NULL, 'القاطع الجانبي اللبني السفلي الأيسر',  'Lower Left Primary Lateral Incisor',  'Lower', 'Left',  'Incisor',  TRUE, 2),
(73, 73, NULL, 'الناب اللبني السفلي الأيسر',             'Lower Left Primary Canine',           'Lower', 'Left',  'Canine',   TRUE, 3),
(74, 74, NULL, 'الضاحك الأول اللبني السفلي الأيسر',    'Lower Left Primary First Molar',      'Lower', 'Left',  'Molar',    TRUE, 4),
(75, 75, NULL, 'الضاحك الثاني اللبني السفلي الأيسر',   'Lower Left Primary Second Molar',     'Lower', 'Left',  'Molar',    TRUE, 5),

-- Primary Teeth (81-85 Lower Right)
(81, 81, NULL, 'القاطع المركزي اللبني السفلي الأيمن',  'Lower Right Primary Central Incisor', 'Lower', 'Right', 'Incisor',  TRUE, 1),
(82, 82, NULL, 'القاطع الجانبي اللبني السفلي الأيمن',  'Lower Right Primary Lateral Incisor', 'Lower', 'Right', 'Incisor',  TRUE, 2),
(83, 83, NULL, 'الناب اللبني السفلي الأيمن',             'Lower Right Primary Canine',          'Lower', 'Right', 'Canine',   TRUE, 3),
(84, 84, NULL, 'الضاحك الأول اللبني السفلي الأيمن',    'Lower Right Primary First Molar',     'Lower', 'Right', 'Molar',    TRUE, 4),
(85, 85, NULL, 'الضاحك الثاني اللبني السفلي الأيمن',   'Lower Right Primary Second Molar',    'Lower', 'Right', 'Molar',    TRUE, 5);

-- Total: 52 teeth (32 permanent + 20 primary)
