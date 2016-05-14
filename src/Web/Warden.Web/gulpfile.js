/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    sass = require('gulp-sass'),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    ignore = require("gulp-ignore"),
    util = require('gulp-util');

var paths = {
  webroot: "./wwwroot/"
};

paths.js = paths.webroot + "content/js/*.js";
paths.minJs = paths.webroot + "content/js/*.min.js";
paths.css = paths.webroot + "content/css/*.css";
paths.scss = paths.webroot + "content/css/*.scss";
paths.minCss = paths.webroot + "content/css/*.min.css";
paths.concatJsDest = paths.webroot + "content/js/site.min.js";
paths.concatCssDest = paths.webroot + "content/css/site.min.css";
paths.concatScssDest = paths.webroot + "content/css/";

gulp.task('sass-compile', function () {
  gulp.src(paths.scss)
      .pipe(sass())
      .pipe(gulp.dest(paths.concatScssDest));
});

gulp.task("clean:js", function (cb) {
  rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
  rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
  return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
      .pipe(concat(paths.concatJsDest))
       .pipe(ignore.exclude(["**/*.map"]))
      .pipe(uglify().on('error', util.log))
      .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
  return gulp.src([paths.css, "!" + paths.minCss])
      .pipe(concat(paths.concatCssDest))
      .pipe(cssmin())
      .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task('default', ['sass-compile', 'clean', 'min'], function () {
});