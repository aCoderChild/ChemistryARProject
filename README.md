# ChemistryARProject

An augmented reality (AR) application built in Unity to visualize chemical elements in 3D space, showing atoms with electron shells (tori) and orbiting electrons. Ideal for educational purposes or chemistry enthusiasts, with support for AR on Android (ARCore) and iOS (ARKit).

## Features
- Visualize atoms and electron shells in AR.
- Add new elements with configurable electron orbits.
- Interactive electron motion using Particle Systems.
- Scalable for adding more elements.

## Usage
- **Scene Overview**: The main scene has an "Atom" GameObject with elements (e.g., sulfur, copper, zinc). Each element has tori (e.g., "Torus_C") for electron shells and orbiting electrons (e.g., "Electron - C1"). Copper has multiple shells (2, 8, 18, 1 electrons) per the Bohr model.
- **Adding Elements**:
  1. Duplicate an element (e.g., "copper") in the Hierarchy.
  2. Rename it (e.g., "sodium").
  3. Create a new material with an appropriate color (e.g., silver for sodium).
  4. Add tori for shells: duplicate a torus, adjust scale (e.g., `0.4` for inner, `1.0` for outer).
  5. Add electrons: duplicate an electron GameObject, set **Max Particles** (e.g., 11 for sodium), and configure `TorusElectronMovement` with the correct torus and radius.
- **Testing in AR**: Deploy to an AR device, anchor the visualization on a flat surface, and interact with elements in 3D.

## Project Structure
- **Assets/**: Unity assets (scenes, scripts, materials, prefabs).
  - **Scenes/**: Main scene with atom visualization.
  - **Scripts/**: `TorusElectronMovement.cs` for electron orbits, `ElectronCreator.cs` for automation.
  - **Materials/**: Element and torus materials (e.g., `CopperMaterial`).
- **Library/**: Unity-generated files (excluded via `.gitignore`).
- **Packages/**: Unity package dependencies.

## Contributing
1. Clone the repo: `git clone https://github.com/aCoderChild/ChemistryARProject.git`.
2. Create a branch: `git checkout -b your-feature-branch`.
3. Make changes and commit: `git commit -m "Add feature"`.
4. Push: `git push origin your-feature-branch`.
5. Open a pull request to `main` or `H_elements`.

**Note**: Track large files (>100 MB) with Git LFS: `git lfs track "path/to/large/file"`.

## Known Issues
- **Large Files**: Files like `Library/ArtifactDB` may exceed GitHub’s 100 MB limit. Use Git LFS: `git lfs migrate import --include="Library/ArtifactDB" --everything`.
- **Lighting**: Rebuild lighting data in Unity if errors occur: **Window** > **Rendering** > **Lighting** > **Generate Lighting**.
- **AR Compatibility**: Test on ARCore/ARKit-supported devices.

## License
MIT License. See `LICENSE` for details.

## Acknowledgments
- Original project by [aCoderChild](https://github.com/aCoderChild).
- Contributions by [HieuNGN](https://github.com/HieuNGN) for new elements and AR enhancements.
- Built with Unity, ARFoundation, ARCore, and ARKit.