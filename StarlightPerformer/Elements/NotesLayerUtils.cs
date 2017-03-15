using System;
using System.Drawing;
using StarlightPerformer.Beatmap;
using StarlightPerformer.Elements.Internal;
using StarlightPerformer.Stage;

namespace StarlightPerformer.Elements {
    internal static class NotesLayerUtils {

        public static void GetBezierFromQuadratic(float x1, float xmid, float x4, out float x2, out float x3) {
            float xcontrol = xmid * 2f - (x1 + x4) * 0.5f;
            x2 = (x1 + xcontrol * 2f) / 3f;
            x3 = (x4 + xcontrol * 2f) / 3f;
        }

        public static void GetNotePairPositions(RenderContext context, double now, Note note1, Note note2, out float x1, out float x2, out float y1, out float y2) {
            var clientSize = context.ClientSize;
            if (IsNotePassed(note1, now)) {
                x1 = GetEndXByNotePosition(clientSize, note1.FinishPosition);
                y1 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note1, now)) {
                x1 = GetStartXByNotePosition(clientSize, note1.FinishPosition);
                y1 = GetBirthYPosition(clientSize);
            } else {
                x1 = GetNoteXPosition(context, now, note1);
                y1 = GetNoteYPosition(context, now, note1);
            }
            if (IsNotePassed(note2, now)) {
                x2 = GetEndXByNotePosition(clientSize, note2.FinishPosition);
                y2 = GetAvatarYPosition(clientSize);
            } else if (IsNoteComing(note2, now)) {
                x2 = GetStartXByNotePosition(clientSize, note2.FinishPosition);
                y2 = GetBirthYPosition(clientSize);
            } else {
                x2 = GetNoteXPosition(context, now, note2);
                y2 = GetNoteYPosition(context, now, note2);
            }
        }

        public static float NoteTimeTransform(float timeRemainingInWindow) {
            return timeRemainingInWindow / (2f - timeRemainingInWindow);
        }

        public static float NoteXTransform(float timeTransformed) {
            return timeTransformed;
        }

        public static float NoteYTransform(float timeTransformed) {
            return timeTransformed + 2.05128205f * timeTransformed * (1f - timeTransformed);
        }

        public static float GetNoteTransformedTime(double now, Note note, bool clampComing = false, bool clampPassed = false) {
            return GetNoteTransformedTime(now, note.HitTiming, clampComing, clampPassed);
        }

        public static float GetNoteTransformedTime(double now, double hitTiming, bool clampComing = false, bool clampPassed = false) {
            var timeRemaining = hitTiming - now;
            var timeRemainingInWindow = (float)timeRemaining / NotesLayer.FutureTimeWindow;
            if (clampComing && timeRemaining > NotesLayer.FutureTimeWindow) {
                timeRemainingInWindow = 1f;
            }
            if (clampPassed && timeRemaining < 0f) {
                timeRemainingInWindow = 0f;
            }
            return NoteTimeTransform(timeRemainingInWindow);
        }

        public static float GetNoteXPosition(RenderContext context, double now, double hitTiming, NotePosition finishPosition, bool clampComing = false, bool clampPassed = false) {
            var timeTransformed = GetNoteTransformedTime(now, hitTiming, clampComing, clampPassed);
            return GetNoteXPosition(context, finishPosition, timeTransformed);
        }

        public static float GetNoteXPosition(RenderContext context, double now, Note note, bool clampComing = false, bool clampPassed = false) {
            var timeTransformed = GetNoteTransformedTime(now, note, clampComing, clampPassed);
            return GetNoteXPosition(context, note.FinishPosition, timeTransformed);
        }

        public static float GetNoteXPosition(RenderContext context, NotePosition finishPosition, float timeTransformed) {
            var clientSize = context.ClientSize;
            var endPos = NotesLayer.AvatarCenterXEndPositions[(int)finishPosition - 1] * clientSize.Width;
            var displayStartPosition = finishPosition;
            var startPos = NotesLayer.AvatarCenterXStartPositions[(int)displayStartPosition - 1] * clientSize.Width;
            return endPos - (endPos - startPos) * NoteXTransform(timeTransformed);
        }

        public static float GetNoteYPosition(RenderContext context, double now, double hitTiming, bool clampComing = false, bool clampPassed = false) {
            var timeTransformed = GetNoteTransformedTime(now, hitTiming, clampComing, clampPassed);
            return GetNoteYPosition(context, timeTransformed);
        }

        public static float GetNoteYPosition(RenderContext context, double now, Note note, bool clampComing = false, bool clampPassed = false) {
            var timeTransformed = GetNoteTransformedTime(now, note, clampComing, clampPassed);
            return GetNoteYPosition(context, timeTransformed);
        }

        public static float GetNoteYPosition(RenderContext context, float timeTransformed) {
            var clientSize = context.ClientSize;
            float ceiling = NotesLayer.FutureNoteCeiling * clientSize.Height,
                baseLine = NotesLayer.BaseLineYPosition * clientSize.Height;
            return baseLine - (baseLine - ceiling) * NoteYTransform(timeTransformed);
        }

        public static float GetNoteRadius(double now, Note note) => GetNoteRadius(now, note.HitTiming);

        public static float GetNoteRadius(double now, double hitTiming) {
            var timeRemaining = hitTiming - now;
            var timeTransformed = NoteTimeTransform((float)timeRemaining / NotesLayer.FutureTimeWindow);
            if (timeTransformed < 0.75f) {
                if (timeTransformed < 0f) {
                    return NotesLayer.AvatarCircleRadius;
                } else {
                    return NotesLayer.AvatarCircleRadius * (1f - timeTransformed * 0.933333333f);
                }
            } else {
                if (timeTransformed < 1f) {
                    return NotesLayer.AvatarCircleRadius * ((1f - timeTransformed) * 1.2f);
                } else {
                    return 0f;
                }
            }
        }

        public static float GetAvatarYPosition(Size clientSize) {
            return clientSize.Height * NotesLayer.BaseLineYPosition;
        }

        public static float GetStartXByNotePosition(Size clientSize, NotePosition position) {
            return clientSize.Width * NotesLayer.AvatarCenterXStartPositions[(int)position - 1];
        }

        public static float GetEndXByNotePosition(Size clientSize, NotePosition position) {
            return clientSize.Width * NotesLayer.AvatarCenterXEndPositions[(int)position - 1];
        }

        public static float GetBirthYPosition(Size clientSize) {
            return clientSize.Height * NotesLayer.FutureNoteCeiling;
        }

        public static OnStageStatus GetNoteOnStageStatus(Note note, double now) {
            if (note.HitTiming < now) {
                return OnStageStatus.Passed;
            }
            if (note.HitTiming > now + NotesLayer.FutureTimeWindow) {
                return OnStageStatus.Upcoming;
            }
            return OnStageStatus.OnStage;
        }

        public static bool IsNoteOnStage(Note note, double now) {
            return now <= note.HitTiming && note.HitTiming <= now + NotesLayer.FutureTimeWindow;
        }

        public static bool IsNotePassed(Note note, double now) {
            return note.HitTiming < now;
        }

        public static bool IsNoteComing(Note note, double now) {
            return note.HitTiming > now + NotesLayer.FutureTimeWindow;
        }

        public static (float x, float y) GetIconLocation(SongColor songColor, Note note) {
            var column = 3;
            switch (songColor) {
                case SongColor.All:
                    column = 3;
                    break;
                case SongColor.Cute:
                    column = 0;
                    break;
                case SongColor.Cool:
                    column = 1;
                    break;
                case SongColor.Passion:
                    column = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(songColor));
            }
            var row = 0;
            if (note.IsTap) {
                row = 0;
            } else if (note.IsHoldStart) {
                row = 1;
            } else if (note.FlickType == NoteFlickType.Left) {
                row = 2;
            } else if (note.FlickType == NoteFlickType.Right) {
                row = 3;
            } /*else if (note.ShouldBeRenderedAsSlide) {
                // Not implemented, don't use.
                row = 4;
            }*/

            return (x: NotesLayer.ImageCellWidth * column, y: NotesLayer.ImageCellHeight * row);
        }

    }
}
